using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Rumas.Maui.Shared.Services.Api;

internal sealed class TokenSessionCircuitHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenSessionAccessor _tokenSessionAccessor;

    public TokenSessionCircuitHandler(IHttpContextAccessor httpContextAccessor, ITokenSessionAccessor tokenSessionAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenSessionAccessor = tokenSessionAccessor;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _tokenSessionAccessor.SessionId ??= _httpContextAccessor.HttpContext?.Request.Cookies["fsh_session_id"];
        return Task.CompletedTask;
    }
}
