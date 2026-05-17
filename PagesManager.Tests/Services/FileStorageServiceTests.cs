using FluentAssertions;
using PagesManager.Core.Services;

namespace PagesManager.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _tempDir;

    public FileStorageServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PagesManagerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task SaveAsync_ShouldSaveFileToStorage()
    {
        var service = new FileStorageService(_tempDir);
        await using var stream = new MemoryStream([1, 2, 3, 4]);

        var path = await service.SaveAsync(stream, "test.png");

        File.Exists(path).Should().BeTrue();
        Path.GetExtension(path).Should().Be(".png");

        var bytes = await File.ReadAllBytesAsync(path);
        bytes.Should().Equal([1, 2, 3, 4]);
    }

    [Fact]
    public async Task SaveAsync_WhenStreamIsNull_ShouldThrow()
    {
        var service = new FileStorageService(_tempDir);

        Func<Task> act = async () => await service.SaveAsync(null!, "test.png");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_WhenFileNameIsEmpty_ShouldThrow()
    {
        var service = new FileStorageService(_tempDir);
        await using var stream = new MemoryStream([1, 2, 3]);

        Func<Task> act = async () => await service.SaveAsync(stream, "");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Delete_ShouldDeleteExistingFile()
    {
        var service = new FileStorageService(_tempDir);
        await using var stream = new MemoryStream([1, 2, 3]);

        var path = await service.SaveAsync(stream, "test.png");

        service.Delete(path);

        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public void Delete_WhenPathIsEmpty_ShouldNotThrow()
    {
        var service = new FileStorageService(_tempDir);

        Action act = () => service.Delete("");

        act.Should().NotThrow();
    }

    [Fact]
    public void GetStorageRoot_ShouldReturnRoot()
    {
        var service = new FileStorageService(_tempDir);

        service.GetStorageRoot().Should().Be(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}