using System;
using Avalonia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PagesManager.Core;
using PagesManager.Core.Data;

namespace PagesManager;

internal class Program
{
    public static IHost? AppHost { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        AppHost = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services
                    .AddPagesManagerCore()
                    .AddPagesManagerViewModels();
            })
            .Build();

        using (var scope = AppHost.Services.CreateScope())
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