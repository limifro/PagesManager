using System.Threading.Tasks;

namespace PagesManager.Core.Services;

public interface IImagePreviewService
{
    Task ShowAsync(string filePath, string title = "");
}