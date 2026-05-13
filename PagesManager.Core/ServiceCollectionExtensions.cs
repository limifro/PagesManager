using System;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PagesManager.Core.Data;
using PagesManager.Core.Helpers;
using PagesManager.Core.Services;
using PagesManager.Core.ViewModels;

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
            options.UseSqlite($"Data Source={path}"),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

services.AddTransient<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileStorageService>(_ => new FileStorageService());
        services.AddTransient<INoteRepository, NoteRepository>();
        services.AddTransient<INoteService, NoteService>();

        return services;
    }

    public static IServiceCollection AddPagesManagerViewModels(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<NoteListViewModel>();
        services.AddSingleton<NoteEditorViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}