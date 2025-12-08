using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudBlazor;
namespace Rumas.Maui.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHeroUI(this IServiceCollection services)
    {
        services.AddMudServices(options =>
        {
            options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            options.SnackbarConfiguration.ShowCloseIcon = true;
            options.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            options.SnackbarConfiguration.MaxDisplayedSnackbars = 3;
        });

        services.AddMudPopoverService();
        services.AddScoped<Rumas.Maui.Shared.Components.Feedback.Snackbar.RumasSnackbar>();
        services.AddSingleton(Rumas.Maui.Shared.Theme.RumasTheme.Build());

        return services;
    }
}
