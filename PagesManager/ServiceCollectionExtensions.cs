using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PagesManager.ViewModels;

namespace PagesManager;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPagesManagerViewModels(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<NoteListViewModel>();
        services.AddSingleton<NoteEditorViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}