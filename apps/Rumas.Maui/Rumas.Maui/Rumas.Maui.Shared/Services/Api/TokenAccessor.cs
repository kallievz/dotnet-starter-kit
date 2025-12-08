namespace Rumas.Maui.Shared.Services.Api;

internal interface ITokenAccessor
{
    string? AccessToken { get; set; }
    string? RefreshToken { get; set; }
    DateTime? AccessTokenExpiresAt { get; set; }
    DateTime? RefreshTokenExpiresAt { get; set; }
}

internal sealed class TokenAccessor : ITokenAccessor
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
