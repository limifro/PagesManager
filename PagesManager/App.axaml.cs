using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PagesManager.Core.ViewModels;
using PagesManager.Views;

namespace PagesManager;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && Program.AppHost is not null)
        {
            var vm = Program.AppHost.Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };

            await vm.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}