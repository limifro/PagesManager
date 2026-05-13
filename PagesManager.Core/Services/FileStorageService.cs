using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PagesManager.Core.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _root;

    public FileStorageService(string? root = null)
    {
        _root = root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PagesManager",
            "Attachments");

        Directory.CreateDirectory(_root);
    }

    public string GetStorageRoot() => _root;

    public async Task<string> SaveAsync(Stream sourceStream, string originalFileName, CancellationToken ct = default)
    {
        if (sourceStream is null) throw new ArgumentNullException(nameof(sourceStream));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name is required", nameof(originalFileName));

        var ext = Path.GetExtension(originalFileName);
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_root, uniqueName);

        await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await sourceStream.CopyToAsync(fs, ct);

        return fullPath;
    }

    public void Delete(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}