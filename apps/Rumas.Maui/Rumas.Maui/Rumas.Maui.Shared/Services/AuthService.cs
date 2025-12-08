using Rumas.Maui.Shared.Models;

namespace Rumas.Maui.Shared.Services;

/// <summary>
/// Provides authentication services using HTTP client calls to the API.
/// This service is platform-agnostic and works with both MAUI and Web projects.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _accessTokenExpiresAt;
    private DateTimeOffset _refreshTokenExpiresAt;
    private const string DefaultTenant = "root";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Logs in a user with the provided credentials.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string password, string? tenant = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            var tenantId = string.IsNullOrWhiteSpace(tenant) ? DefaultTenant : tenant.Trim();
            var loginRequest = new { email, password };

            // Build URL with tenant header
            var requestUri = "api/v1/identity/token/issue";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("tenant", tenantId);
            
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Login failed with status {response.StatusCode}: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(json);
            if (loginResponse != null)
            {
                StoreTokens(loginResponse);
            }

            return loginResponse;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during login.", ex);
        }
    }

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    public async Task<LoginResponse?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Access token and refresh token are required.");
            }

            var refreshRequest = new { token = accessToken, refreshToken };

            var requestUri = "api/v1/identity/token/refresh";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("tenant", DefaultTenant);
            
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(refreshRequest);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                ClearTokens();
                throw new InvalidOperationException($"Token refresh failed with status {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // For simplicity, create a LoginResponse from the refresh response
            // Assumes the response structure matches TokenResponse
            var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(json);
            
            if (loginResponse != null)
            {
                StoreTokens(loginResponse);
            }

            return loginResponse;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during token refresh.", ex);
        }
    }

    /// <summary>
    /// Gets the stored access token.
    /// </summary>
    public string? GetAccessToken() => _accessToken;

    /// <summary>
    /// Gets the stored refresh token.
    /// </summary>
    public string? GetRefreshToken() => _refreshToken;

    /// <summary>
    /// Stores the tokens from a login response.
    /// </summary>
    public void StoreTokens(LoginResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        _accessToken = response.AccessToken;
        _refreshToken = response.RefreshToken;
        _accessTokenExpiresAt = response.AccessTokenExpiresAt;
        _refreshTokenExpiresAt = response.RefreshTokenExpiresAt;
    }

    /// <summary>
    /// Clears all stored tokens.
    /// </summary>
    public void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
        _accessTokenExpiresAt = DateTimeOffset.MinValue;
        _refreshTokenExpiresAt = DateTimeOffset.MinValue;
    }

    /// <summary>
    /// Checks if the access token is valid (not expired).
    /// </summary>
    public bool IsTokenValid() => !string.IsNullOrWhiteSpace(_accessToken) && DateTimeOffset.UtcNow < _accessTokenExpiresAt;
}
