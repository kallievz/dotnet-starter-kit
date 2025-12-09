using FSH.Rumas.Blazor.ApiClient;
using FSH.Rumas.Blazor.Services.Api;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace FSH.Rumas.Blazor.Services;

internal sealed record BffTokenResponse(
    string AccessToken,
    string RefreshToken,
    System.DateTime RefreshTokenExpiresAt,
    System.DateTime AccessTokenExpiresAt);

internal interface ITokenStore
{
    Task StoreAsync(string subject, BffTokenResponse token, CancellationToken cancellationToken = default);

    Task<BffTokenResponse?> GetAsync(string subject, CancellationToken cancellationToken = default);

    Task RemoveAsync(string subject, CancellationToken cancellationToken = default);
}

internal sealed class InMemoryTokenStore : ITokenStore
{
    private readonly ConcurrentDictionary<string, BffTokenResponse> _tokens = new();

    public Task StoreAsync(string subject, BffTokenResponse token, CancellationToken cancellationToken = default)
    {
        _tokens[subject] = token;
        return Task.CompletedTask;
    }

    public Task<BffTokenResponse?> GetAsync(string subject, CancellationToken cancellationToken = default)
    {
        _tokens.TryGetValue(subject, out var token);
        return Task.FromResult(token);
    }

    public Task RemoveAsync(string subject, CancellationToken cancellationToken = default)
    {
        _tokens.TryRemove(subject, out _);
        return Task.CompletedTask;
    }
}

internal sealed class BffAuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenStore _tokenStore;
    private readonly ITokenSessionAccessor _tokenSessionAccessor;
    private readonly ITokenAccessor _tokenAccessor;

    private const string SessionCookieName = "fsh_session_id";
    private const string TenantCookieName = "fsh_tenant";
    private const string DefaultTenant = "root";

    public BffAuthDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        ITokenStore tokenStore,
        ITokenSessionAccessor tokenSessionAccessor,
        ITokenAccessor tokenAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenStore = tokenStore;
        _tokenSessionAccessor = tokenSessionAccessor;
        _tokenAccessor = tokenAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var sessionId = _tokenSessionAccessor.SessionId ?? httpContext?.Request.Cookies[SessionCookieName];

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            if (_tokenSessionAccessor.SessionId is null)
            {
                _tokenSessionAccessor.SessionId = sessionId;
            }

            var token = _tokenAccessor.AccessToken is not null
                ? new BffTokenResponse(
                    _tokenAccessor.AccessToken,
                    _tokenAccessor.RefreshToken ?? string.Empty,
                    _tokenAccessor.RefreshTokenExpiresAt ?? DateTime.UtcNow,
                    _tokenAccessor.AccessTokenExpiresAt ?? DateTime.UtcNow)
                : await _tokenStore.GetAsync(sessionId, cancellationToken);
            if (token is not null && !string.IsNullOrWhiteSpace(token.AccessToken))
            {
                ArgumentNullException.ThrowIfNull(request);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                if (!request.Headers.Contains("tenant"))
                {
                    var tenant = httpContext?.Request.Cookies[TenantCookieName] ?? DefaultTenant;
                    request.Headers.TryAddWithoutValidation("tenant", tenant);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

internal static class BffAuthEndpoints
{
    private const string SessionCookieName = "fsh_session_id";
    private const string TenantCookieName = "fsh_tenant";
    private const string DefaultTenant = "root";

    public static void MapBffAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", async (
            LoginRequest request,
            ITokenClient tokenClient,
            HttpContext httpContext,
            ITokenSessionAccessor tokenSessionAccessor,
            ITokenAccessor tokenAccessor,
            ITokenStore tokenStore,
            CancellationToken cancellationToken) =>
        {
            var tenant = string.IsNullOrWhiteSpace(request.Tenant) ? DefaultTenant : request.Tenant;

            TokenResponse token;
            try
            {
                token = await tokenClient.IssueAsync(
                    tenant,
                    new GenerateTokenCommand
                    {
                        Email = request.Email,
                        Password = request.Password
                    },
                    cancellationToken);
            }
            catch (ApiException)
            {
                return Results.Unauthorized();
            }
            catch
            {
                return Results.Problem("Failed to reach identity API.");
            }

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return Results.Problem("Invalid token response from identity API.");
            }

            var sessionId = Guid.NewGuid().ToString("N");
            tokenSessionAccessor.SessionId = sessionId;
            tokenAccessor.AccessToken = token.AccessToken;
            tokenAccessor.RefreshToken = token.RefreshToken;
            tokenAccessor.AccessTokenExpiresAt = token.AccessTokenExpiresAt.UtcDateTime;
            tokenAccessor.RefreshTokenExpiresAt = token.RefreshTokenExpiresAt.UtcDateTime;
            await tokenStore.StoreAsync(
                sessionId,
                new BffTokenResponse(
                    token.AccessToken,
                    token.RefreshToken,
                    token.RefreshTokenExpiresAt.UtcDateTime,
                    token.AccessTokenExpiresAt.UtcDateTime),
                cancellationToken);

            var isHttps = httpContext.Request.IsHttps;

            httpContext.Response.Cookies.Append(
                SessionCookieName,
                sessionId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });

            httpContext.Response.Cookies.Append(
                TenantCookieName,
                tenant,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });

            return Results.Ok(new LoginResult(
                sessionId,
                token.AccessToken,
                token.RefreshToken,
                token.AccessTokenExpiresAt.UtcDateTime,
                token.RefreshTokenExpiresAt.UtcDateTime));
        });

        app.MapPost("/auth/logout", async (
            HttpContext httpContext,
            ITokenStore tokenStore,
            CancellationToken cancellationToken) =>
        {
            var sessionId = httpContext.Request.Cookies[SessionCookieName];
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                await tokenStore.RemoveAsync(sessionId, cancellationToken);
            }

            httpContext.Response.Cookies.Delete(SessionCookieName);
            httpContext.Response.Cookies.Delete(TenantCookieName);

            return Results.Ok();
        });

        app.MapGet("/auth/status", async (
            HttpContext httpContext,
            ITokenStore tokenStore,
            CancellationToken cancellationToken) =>
        {
            var sessionId = httpContext.Request.Cookies[SessionCookieName];
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Results.Unauthorized();
            }

            var token = await tokenStore.GetAsync(sessionId, cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return Results.Unauthorized();
            }

            return Results.Ok();
        });

        app.MapGet("/auth/session", async (
            HttpContext httpContext,
            ITokenStore tokenStore,
            CancellationToken cancellationToken) =>
        {
            var sessionId = httpContext.Request.Cookies[SessionCookieName];
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Results.Unauthorized();
            }

            var token = await tokenStore.GetAsync(sessionId, cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new SessionInfoResult(
                sessionId,
                token.AccessToken,
                token.RefreshToken,
                token.AccessTokenExpiresAt,
                token.RefreshTokenExpiresAt));
        });
    }
}

internal sealed record LoginRequest(string Email, string Password, string? Tenant);
internal sealed record LoginResult(
    string SessionId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
internal sealed record SessionInfoResult(
    string SessionId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);