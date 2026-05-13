using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PagesManager.Core.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream sourceStream, string originalFileName, CancellationToken ct = default);
    void Delete(string filePath);
    string GetStorageRoot();
}