using System.Windows;
using System.Windows.Media;
using XP_Hotkey.Models;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace XP_Hotkey.Services;

public class ThemeService
{
    private readonly ConfigService _configService;

    public ThemeService(ConfigService configService)
    {
        _configService = configService;
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        var config = _configService.GetConfig();
        var theme = config.Theme;

        if (theme.UseDarkMode)
        {
            ApplyDarkMode();
        }
        else
        {
            ApplyLightMode();
        }

        ApplyAccentColor(theme.AccentColor);
    }

    private void ApplyDarkMode()
    {
        var darkBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        var lightBrush = new SolidColorBrush(Colors.White);

        Application.Current.Resources["BackgroundColor"] = darkBrush;
        Application.Current.Resources["ForegroundColor"] = lightBrush;
        Application.Current.Resources["PanelBackground"] = new SolidColorBrush(Color.FromRgb(40, 40, 40));
    }

    private void ApplyLightMode()
    {
        var lightBrush = new SolidColorBrush(Colors.White);
        var darkBrush = new SolidColorBrush(Colors.Black);

        Application.Current.Resources["BackgroundColor"] = lightBrush;
        Application.Current.Resources["ForegroundColor"] = darkBrush;
        Application.Current.Resources["PanelBackground"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
    }

    private void ApplyAccentColor(string colorHex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            Application.Current.Resources["AccentColor"] = new SolidColorBrush(color);
        }
        catch
        {
            // Use default color
            Application.Current.Resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0, 120, 212));
        }
    }

    public void ToggleTheme()
    {
        _configService.UpdateConfig(config =>
        {
            config.Theme.UseDarkMode = !config.Theme.UseDarkMode;
        });
        ApplyTheme();
    }
}

