using PagesManager.Core.Services;

namespace PagesManager.Tests.Helpers;

public class FakeImagePreviewService : IImagePreviewService
{
    public List<(string Path, string Title)> Shown { get; } = new();

    public Task ShowAsync(string filePath, string title = "")
    {
        Shown.Add((filePath, title));
        return Task.CompletedTask;
    }
}