using FluentAssertions;
using PagesManager.Core.Models;
using PagesManager.Core.ViewModels;
using PagesManager.Tests.Helpers;

namespace PagesManager.Tests.ViewModels;

public class NoteListItemViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesFromNote()
    {
        var now = new DateTime(2026, 5, 14, 10, 0, 0, DateTimeKind.Utc);
        var clock = new TestClock(now);

        var note = new Note
        {
            Id = 1,
            Title = "Test title",
            Content = "Test content",
            UpdatedAt = now,
            IsPinned = true
        };

        var vm = new NoteListItemViewModel(note, clock);

        vm.Title.Should().Be("Test title");
        vm.Preview.Should().Be("Test content");
        vm.UpdatedAt.Should().Be(now);
        vm.IsPinned.Should().BeTrue();
        vm.PinText.Should().Be("📌 ");
    }

    [Fact]
    public void Constructor_WhenContentIsEmpty_ShouldShowEmptyPreviewText()
    {
        var clock = new TestClock(DateTime.UtcNow);

        var note = new Note
        {
            Title = "Title",
            Content = ""
        };

        var vm = new NoteListItemViewModel(note, clock);

        vm.Preview.Should().Be("Нет дополнительного текста");
    }

    [Fact]
    public void Constructor_WhenContentIsLong_ShouldTrimPreview()
    {
        var clock = new TestClock(DateTime.UtcNow);

        var note = new Note
        {
            Title = "Title",
            Content = new string('a', 100)
        };

        var vm = new NoteListItemViewModel(note, clock);

        vm.Preview.Length.Should().BeLessThan(100);
        vm.Preview.Should().EndWith("…");
    }

    [Fact]
    public void Refresh_ShouldUpdatePropertiesFromModel()
    {
        var clock = new TestClock(DateTime.UtcNow);

        var note = new Note
        {
            Title = "Old",
            Content = "Old content",
            IsPinned = false
        };

        var vm = new NoteListItemViewModel(note, clock);

        note.Title = "New";
        note.Content = "New content";
        note.IsPinned = true;

        vm.Refresh();

        vm.Title.Should().Be("New");
        vm.Preview.Should().Be("New content");
        vm.IsPinned.Should().BeTrue();
        vm.PinText.Should().Be("📌 ");
    }
}