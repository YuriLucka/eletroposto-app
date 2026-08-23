using MudBlazor;

namespace ev_charge_prototype.Theme;

public static class AppTheme
{
    public static MudTheme Default => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0B5FFF",
            Secondary = "#00A86B",
            Tertiary = "#FFB800",
            Background = "#F4F6F8",
            Success = "#2FB86E",
            Error = "#E5484D",
            Warning = "#F5A524",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5B8DEF",
            Secondary = "#22C58B",
            Tertiary = "#FFC94A",
            Background = "#111417",
            Surface = "#1B1F23",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = FontStack },
            H1 = new H1Typography { FontFamily = FontStack },
            H2 = new H2Typography { FontFamily = FontStack },
            H3 = new H3Typography { FontFamily = FontStack },
            H4 = new H4Typography { FontFamily = FontStack },
            H5 = new H5Typography { FontFamily = FontStack },
            H6 = new H6Typography { FontFamily = FontStack },
            Subtitle1 = new Subtitle1Typography { FontFamily = FontStack },
            Subtitle2 = new Subtitle2Typography { FontFamily = FontStack },
            Body1 = new Body1Typography { FontFamily = FontStack },
            Body2 = new Body2Typography { FontFamily = FontStack },
            Button = new ButtonTypography { FontFamily = FontStack },
            Caption = new CaptionTypography { FontFamily = FontStack },
            Overline = new OverlineTypography { FontFamily = FontStack },
        },
    };

    private static readonly string[] FontStack = { "Inter", "Roboto", "Helvetica", "Arial", "sans-serif" };
}
