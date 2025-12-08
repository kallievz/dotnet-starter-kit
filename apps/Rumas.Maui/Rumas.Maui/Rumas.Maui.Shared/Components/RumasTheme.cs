using MudBlazor;

namespace Rumas.Maui.Shared.Theme;

public static class RumasTheme
{
    public static MudTheme Build()
    {
        // Shadcn-inspired, subtle palette: neutral surfaces, soft primary.
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2563EB",           // blue-600
                Secondary = "#0F172A",         // slate-900
                Tertiary = "#6366F1",          // indigo-500
                Background = "#F8FAFC",        // slate-50
                Surface = "#FFFFFF",
                AppbarBackground = "#F8FAFC",
                AppbarText = "#0F172A",
                DrawerBackground = "#FFFFFF",
                TextPrimary = "#0F172A",
                TextSecondary = "#475569",     // slate-600
                Info = "#0284C7",
                Success = "#16A34A",
                Warning = "#F59E0B",
                Error = "#DC2626",
                TableLines = "#E2E8F0",
                Divider = "#E2E8F0"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#38BDF8",           // sky-400
                Secondary = "#94A3B8",         // slate-400
                Tertiary = "#818CF8",          // indigo-400
                Background = "#0B1220",
                Surface = "#111827",
                AppbarBackground = "#0B1220",
                AppbarText = "#E2E8F0",
                DrawerBackground = "#0B1220",
                TextPrimary = "#E2E8F0",
                TextSecondary = "#CBD5E1",     // slate-300
                Info = "#38BDF8",
                Success = "#22C55E",
                Warning = "#FBBF24",
                Error = "#F87171",
                TableLines = "#1F2937",
                Divider = "#1F2937"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "4px"
            }
        };
    }
}
