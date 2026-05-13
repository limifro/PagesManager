using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PagesManager.Core.Services;

public record PickedFile(string FileName, string ContentType, Stream OpenRead);

public interface IFilePickerService
{
    Task<IReadOnlyList<PickedFile>> PickImagesAsync(bool allowMultiple = true);
}