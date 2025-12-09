using FSH.Framework.Blazor.UI;
using FSH.Rumas.Blazor;
using FSH.Rumas.Blazor.Components;
using FSH.Rumas.Blazor.Services;
using FSH.Rumas.Blazor.Services.Api;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelerikBlazor();
builder.Services.AddHeroUI();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITokenStore, InMemoryTokenStore>();
builder.Services.AddScoped<ITokenSessionAccessor, TokenSessionAccessor>();
builder.Services.AddScoped<ITokenAccessor, TokenAccessor>();
builder.Services.AddScoped<CircuitHandler, TokenSessionCircuitHandler>();
builder.Services.AddScoped<BffAuthDelegatingHandler>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
                 ?? throw new InvalidOperationException("Api:BaseUrl configuration is missing.");

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<BffAuthDelegatingHandler>();
    handler.InnerHandler ??= new HttpClientHandler();
    return new HttpClient(handler, disposeHandler: false)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

builder.Services.AddApiClients(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Simple health endpoints for ALB/ECS
app.MapGet("/health/ready", () => Results.Ok(new { status = "Healthy" }))
   .AllowAnonymous();

app.MapGet("/health/live", () => Results.Ok(new { status = "Alive" }))
   .AllowAnonymous();

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapBffAuthEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

await app.RunAsync();