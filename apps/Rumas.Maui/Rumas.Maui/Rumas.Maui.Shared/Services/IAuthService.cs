using Rumas.Maui.Shared.Models;

namespace Rumas.Maui.Shared.Services;

/// <summary>
/// Defines authentication service contract for login and token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Logs in a user with the provided credentials.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="tenant">The tenant identifier (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A login response containing tokens if successful.</returns>
    Task<LoginResponse?> LoginAsync(string email, string password, string? tenant = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the access token using a refresh token.
    /// </summary>
    /// <param name="accessToken">The current access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A login response with new tokens if successful.</returns>
    Task<LoginResponse?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the stored access token.
    /// </summary>
    string? GetAccessToken();

    /// <summary>
    /// Gets the stored refresh token.
    /// </summary>
    string? GetRefreshToken();

    /// <summary>
    /// Stores the tokens from a login response.
    /// </summary>
    /// <param name="response">The login response containing tokens.</param>
    void StoreTokens(LoginResponse response);

    /// <summary>
    /// Clears all stored tokens.
    /// </summary>
    void ClearTokens();

    /// <summary>
    /// Checks if the access token is valid (not expired).
    /// </summary>
    bool IsTokenValid();
}
