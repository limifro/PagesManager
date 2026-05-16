using PagesManager.Core.Services;

namespace PagesManager.Tests.Helpers;

public class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public IReadOnlyDictionary<string, byte[]> Files => _files;
    public List<string> DeletedFiles { get; } = new();

    public async Task<string> SaveAsync(Stream sourceStream, string originalFileName, CancellationToken ct = default)
    {
        var path = $"/fake/storage/{Guid.NewGuid()}_{originalFileName}";

        using var ms = new MemoryStream();
        await sourceStream.CopyToAsync(ms, ct);

        _files[path] = ms.ToArray();

        return path;
    }

    public void Delete(string filePath)
    {
        DeletedFiles.Add(filePath);
        _files.Remove(filePath);
    }

    public string GetStorageRoot()
    {
        return "/fake/storage";
    }
}