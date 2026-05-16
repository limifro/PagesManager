using System;
using System.IO;
using Avalonia;
using Avalonia.Styling;
using PagesManager.Core.Services;

namespace PagesManager.Services;

public class AvaloniaThemeService : IThemeService
{
    private readonly string _settingsPath;

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public AvaloniaThemeService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PagesManager");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "theme.txt");
    }

    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = theme == AppTheme.Dark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        try { File.WriteAllText(_settingsPath, theme.ToString()); }
        catch
        {
            
        }
    }

    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }

    public AppTheme LoadSavedTheme()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var text = File.ReadAllText(_settingsPath).Trim();
                if (Enum.TryParse<AppTheme>(text, out var saved))
                {
                    CurrentTheme = saved;
                    return saved;
                }
            }
        }
        catch
        {
            
        }
        return AppTheme.Light;
    }
}