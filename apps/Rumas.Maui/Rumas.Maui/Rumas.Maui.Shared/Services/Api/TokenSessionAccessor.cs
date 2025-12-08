using Microsoft.AspNetCore.Http;

namespace Rumas.Maui.Shared.Services.Api;

internal interface ITokenSessionAccessor
{
    string? SessionId { get; set; }
}

internal sealed class TokenSessionAccessor : ITokenSessionAccessor
{
    public string? SessionId { get; set; }

    public TokenSessionAccessor(IHttpContextAccessor httpContextAccessor)
    {
        SessionId = httpContextAccessor.HttpContext?.Request.Cookies["fsh_session_id"];
    }
}
