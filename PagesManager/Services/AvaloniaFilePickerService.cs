using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using PagesManager.Core.Services;

namespace PagesManager.Services;

public class AvaloniaFilePickerService : IFilePickerService
{
    public async Task<IReadOnlyList<PickedFile>> PickImagesAsync(bool allowMultiple = true)
    {
        var topLevel = GetTopLevel();
        if (topLevel is null) return new List<PickedFile>();

        var options = new FilePickerOpenOptions
        {
            Title = "Выберите изображение",
            AllowMultiple = allowMultiple,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.webp" },
                    AppleUniformTypeIdentifiers = new[] { "public.image" },
                    MimeTypes = new[] { "image/*" }
                }
            }
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files is null || files.Count == 0)
            return new List<PickedFile>();

        var result = new List<PickedFile>();
        foreach (var file in files)
        {
            var stream = await file.OpenReadAsync();
            var contentType = GuessContentType(file.Name);
            result.Add(new PickedFile(file.Name, contentType, stream));
        }

        return result;
    }

    private static TopLevel? GetTopLevel()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private static string GuessContentType(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}