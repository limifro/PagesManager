using FluentAssertions;
using PagesManager.Core.Models;
using PagesManager.Core.ViewModels;

namespace PagesManager.Tests.ViewModels;

public class AttachmentViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeFromModel()
    {
        var model = new Attachment
        {
            Id = 5,
            FilePath = "/path/to/file.png",
            FileName = "file.png"
        };

        var vm = new AttachmentViewModel(model);

        vm.Id.Should().Be(5);
        vm.FilePath.Should().Be("/path/to/file.png");
        vm.FileName.Should().Be("file.png");
        vm.Model.Should().BeSameAs(model);
    }

    [Fact]
    public void FilePath_CanBeChanged()
    {
        var model = new Attachment { FilePath = "/old/path.png", FileName = "x.png" };
        var vm = new AttachmentViewModel(model);

        vm.FilePath = "/new/path.png";

        vm.FilePath.Should().Be("/new/path.png");
    }
}