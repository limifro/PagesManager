using PagesManager.Core.Services;

namespace PagesManager.Tests.Helpers;

public class FakeFilePickerService : IFilePickerService
{
    public List<PickedFile> NextResult { get; set; } = new();
    public int CallCount { get; private set; }

    public Task<IReadOnlyList<PickedFile>> PickImagesAsync(bool allowMultiple = true)
    {
        CallCount++;
        return Task.FromResult<IReadOnlyList<PickedFile>>(NextResult);
    }
}