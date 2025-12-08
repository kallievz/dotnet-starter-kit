using Rumas.Maui.Web.Components;
using Rumas.Maui.Shared.Services;
using Rumas.Maui.Web.Services;
using Rumas.Maui.Shared.Models;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

// Add device-specific services used by the Rumas.Maui.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Add API clients
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] 
    ?? throw new InvalidOperationException("Api:BaseUrl configuration is missing.");

builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Rumas.Maui.Shared._Imports).Assembly,
        typeof(Rumas.Maui.Web.Client._Imports).Assembly);

app.Run();
