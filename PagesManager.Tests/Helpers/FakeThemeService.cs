using PagesManager.Core.Services;

namespace PagesManager.Tests.Helpers;

public class FakeThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;
    public int ToggleCount { get; private set; }

    public void SetTheme(AppTheme theme) => CurrentTheme = theme;

    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        ToggleCount++;
    }

    public AppTheme LoadSavedTheme() => CurrentTheme;
}