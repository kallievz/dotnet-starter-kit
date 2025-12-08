namespace Rumas.Maui.Shared.Models;

/// <summary>
/// Represents a login request with user credentials.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant identifier (optional).
    /// </summary>
    public string? Tenant { get; set; }
}
