using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PagesManager.Core.Data;
using PagesManager.Core.Helpers;
using PagesManager.Core.Services;

namespace PagesManager.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPagesManagerCore(this IServiceCollection services, string? dbPath = null)
    {
        var path = dbPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PagesManager",
            "pagesmanager.db");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={path}"));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileStorageService>(_ => new FileStorageService());
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();

        return services;
    }
}