using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PagesManager.Core.Services;

namespace PagesManager.Services;

public class AvaloniaImagePreviewService : IImagePreviewService
{
    public async Task ShowAsync(string filePath, string title = "")
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        await using var stream = File.OpenRead(filePath);
        var bitmap = new Bitmap(stream);

        var image = new Image
        {
            Source = bitmap,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var window = new Window
        {
            Width = 900,
            Height = 700,
            Title = string.IsNullOrWhiteSpace(title) ? "Просмотр изображения" : title,
            Background = Brushes.Black,
            Content = new ScrollViewer
            {
                Content = image,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            await window.ShowDialog(desktop.MainWindow);
        }
        else
        {
            window.Show();
        }
    }
}