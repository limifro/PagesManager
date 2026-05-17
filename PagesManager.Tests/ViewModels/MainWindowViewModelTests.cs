using FluentAssertions;
using PagesManager.Core.Messages;
using PagesManager.Core.Models;
using PagesManager.Core.Services;
using PagesManager.Core.ViewModels;
using PagesManager.Tests.Helpers;

namespace PagesManager.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static MainWindowViewModel CreateVm(
        out NoteService service,
        out MessengerSpy spy,
        out FakeThemeService themeService)
    {
        var db = TestDbContextFactory.Create();
        var clock = new TestClock(DateTime.UtcNow);
        var storage = new FakeFileStorageService();
        service = new NoteService(storage, db, clock);

        spy = new MessengerSpy();
        themeService = new FakeThemeService();

        var listVm = new NoteListViewModel(service, spy.Messenger) { SearchDebounceMs = 0 };
        var editorVm = new NoteEditorViewModel(
            service,
            new FakeFilePickerService(),
            new FakeImagePreviewService(),
            storage,
            spy.Messenger);

        return new MainWindowViewModel(listVm, editorVm, spy.Messenger, themeService);
    }

    [Fact]
    public void Constructor_ShouldExposeSubViewModels()
    {
        var vm = CreateVm(out _, out _, out _);

        vm.NoteList.Should().NotBeNull();
        vm.NoteEditor.Should().NotBeNull();
    }

    [Fact]
    public async Task InitializeAsync_ShouldLoadNotes()
    {
        var vm = CreateVm(out var service, out _, out _);
        await service.CreateAsync("X");

        await vm.InitializeAsync();

        vm.NoteList.Notes.Should().HaveCount(1);
    }

    [Fact]
    public void ToggleTheme_ShouldCallThemeServiceAndUpdateFlag()
    {
        var vm = CreateVm(out _, out _, out var themeService);

        vm.IsDarkTheme.Should().BeFalse();

        vm.ToggleThemeCommand.Execute(null);

        themeService.ToggleCount.Should().Be(1);
        vm.IsDarkTheme.Should().BeTrue();

        vm.ToggleThemeCommand.Execute(null);

        themeService.ToggleCount.Should().Be(2);
        vm.IsDarkTheme.Should().BeFalse();
    }

    [Fact]
    public async Task WhenNoteSelected_EditorShouldLoadIt()
    {
        var vm = CreateVm(out var service, out _, out _);

        var note = await service.CreateAsync("X");
        await vm.InitializeAsync();

        vm.NoteList.SelectedNote = vm.NoteList.Notes.First();

        vm.NoteEditor.CurrentNote.Should().NotBeNull();
        vm.NoteEditor.CurrentNote!.Id.Should().Be(note.Id);
    }
}