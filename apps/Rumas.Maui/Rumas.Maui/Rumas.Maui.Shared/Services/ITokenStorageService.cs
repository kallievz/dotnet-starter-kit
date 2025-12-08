using Rumas.Maui.Shared.Models;

namespace Rumas.Maui.Shared.Services;

/// <summary>
/// Interface for persistent token storage.
/// Implementations will handle platform-specific storage (SecureStorage for MAUI, LocalStorage for Web).
/// </summary>
public interface ITokenStorageService
{
    /// <summary>
    /// Saves the login response tokens to persistent storage.
    /// </summary>
    Task SaveTokensAsync(LoginResponse response, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the stored login response from persistent storage.
    /// </summary>
    Task<LoginResponse?> GetTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all stored tokens from persistent storage.
    /// </summary>
    Task ClearTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if valid tokens are stored.
    /// </summary>
    Task<bool> HasValidTokensAsync(CancellationToken cancellationToken = default);
}
