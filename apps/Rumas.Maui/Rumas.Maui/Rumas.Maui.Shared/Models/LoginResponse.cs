namespace Rumas.Maui.Shared.Models;

/// <summary>
/// Represents the response from a successful login operation.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time when the access token expires.
    /// </summary>
    public DateTimeOffset AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the time when the refresh token expires.
    /// </summary>
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    public string? SessionId { get; set; }
}
