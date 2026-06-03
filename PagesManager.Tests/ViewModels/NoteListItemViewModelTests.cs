using System;
using FluentAssertions;
using PagesManager.Core.Models;
using PagesManager.ViewModels;

namespace PagesManager.Tests.ViewModels;

public class NoteListItemViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesFromNote()
    {
        var note = new Note
        {
            Id = 1,
            Title = "Test title",
            Content = "Test content",
            UpdatedAt = DateTime.UtcNow,
            IsPinned = true
        };

        var vm = new NoteListItemViewModel(note);

        vm.Title.Should().Be("Test title");
        vm.Preview.Should().Be("Test content");
        vm.IsPinned.Should().BeTrue();
        vm.PinText.Should().Be("📌 ");
    }

    [Fact]
    public void Constructor_WhenContentIsEmpty_ShouldShowEmptyPreviewText()
    {
        var note = new Note { Title = "Title", Content = "" };

        var vm = new NoteListItemViewModel(note);

        vm.Preview.Should().Be("Нет дополнительного текста");
    }

    [Fact]
    public void Constructor_WhenContentIsLong_ShouldTrimPreview()
    {
        var note = new Note { Title = "Title", Content = new string('a', 100) };

        var vm = new NoteListItemViewModel(note);

        vm.Preview.Length.Should().BeLessThan(100);
        vm.Preview.Should().EndWith("…");
    }

    [Fact]
    public void Refresh_ShouldUpdatePropertiesFromModel()
    {
        var note = new Note { Title = "Old", Content = "Old content", IsPinned = false };

        var vm = new NoteListItemViewModel(note);

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