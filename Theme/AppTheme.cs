using MudBlazor;

namespace ev_charge_prototype.Theme;

public static class AppTheme
{
    public static MudTheme Default => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#00A86B",
            Secondary = "#0B5FFF",
            Tertiary = "#FFB800",
            Background = "#F4F6F8",
            Success = "#2FB86E",
            Error = "#E5484D",
            Warning = "#F5A524",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#22C58B",
            Secondary = "#5B8DEF",
            Tertiary = "#FFC94A",
            Background = "#111417",
            Surface = "#1B1F23",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
        }
    };
}
