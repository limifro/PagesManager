using System;
using Avalonia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PagesManager.Core;
using PagesManager.Core.Data;
using PagesManager.Core.Services;
using PagesManager.Services;

namespace PagesManager;

internal class Program
{
    public static IServiceProvider? Services { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        var collection = new ServiceCollection();

        collection.AddPagesManagerCore();
        collection.AddPagesManagerViewModels();
        collection.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        collection.AddSingleton<IImagePreviewService, AvaloniaImagePreviewService>();
        collection.AddSingleton<IThemeService, AvaloniaThemeService>();

        Services = collection.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}