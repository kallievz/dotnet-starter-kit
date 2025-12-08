using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Rumas.Maui.Shared;
using Rumas.Maui.Shared.Services;
using Rumas.Maui.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Add device-specific services used by the Rumas.Maui.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();


// Add API clients
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] 
    ?? "https://localhost:7030";

builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
