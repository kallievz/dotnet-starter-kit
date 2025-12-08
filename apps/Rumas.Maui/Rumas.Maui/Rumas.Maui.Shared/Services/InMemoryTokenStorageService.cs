using Rumas.Maui.Shared.Models;
using System.Text.Json;

namespace Rumas.Maui.Shared.Services;

/// <summary>
/// Memory-based token storage service (default implementation for testing/development).
/// For production MAUI apps, use SecureStorageTokenService.
/// For web apps, use LocalStorageTokenService (requires JS interop or local storage wrapper).
/// </summary>
public class InMemoryTokenStorageService : ITokenStorageService
{
    private LoginResponse? _cachedTokens;

    /// <summary>
    /// Saves tokens to in-memory cache.
    /// </summary>
    public Task SaveTokensAsync(LoginResponse response, CancellationToken cancellationToken = default)
    {
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        _cachedTokens = response;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves tokens from in-memory cache.
    /// </summary>
    public Task<LoginResponse?> GetTokensAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cachedTokens);
    }

    /// <summary>
    /// Clears in-memory token cache.
    /// </summary>
    public Task ClearTokensAsync(CancellationToken cancellationToken = default)
    {
        _cachedTokens = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if cached tokens are still valid.
    /// </summary>
    public Task<bool> HasValidTokensAsync(CancellationToken cancellationToken = default)
    {
        var hasTokens = _cachedTokens != null && 
                       !string.IsNullOrWhiteSpace(_cachedTokens.AccessToken) &&
                       DateTimeOffset.UtcNow < _cachedTokens.AccessTokenExpiresAt;
        return Task.FromResult(hasTokens);
    }
}
