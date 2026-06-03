using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PagesManager.Core.Messages;
using PagesManager.Core.Models;
using PagesManager.Core.Services;
using PagesManager.ViewModels;
using PagesManager.Tests.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace PagesManager.Tests.ViewModels;

public class NoteListViewModelTests
{
    private static NoteListViewModel CreateVm(out NoteService service, out MessengerSpy spy)
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        service = new NoteService(storage, db);
        spy = new MessengerSpy();
        spy.RegisterAll<NoteSelectedMessage>();

        return new NoteListViewModel(service, spy.Messenger) { SearchDebounceMs = 0 };
    }

    [Fact]
    public async Task LoadAsync_ShouldFillNotesCollection()
    {
        var vm = CreateVm(out var service, out _);

        await service.CreateAsync("First");
        await service.CreateAsync("Second");

        await vm.LoadAsync();

        vm.Notes.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateNoteAsync_ShouldAddNewNoteAndSelectIt()
    {
        var vm = CreateVm(out _, out var spy);

        await vm.CreateNoteCommand.ExecuteAsync(null);

        vm.Notes.Should().HaveCount(1);
        vm.SelectedNote.Should().NotBeNull();
        spy.OfType<NoteSelectedMessage>().Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteSelectedAsync_ShouldDeleteSelectedNote()
    {
        var vm = CreateVm(out var service, out _);

        var note = await service.CreateAsync();
        await vm.LoadAsync();

        vm.SelectedNote = vm.Notes.First();
        await vm.DeleteSelectedCommand.ExecuteAsync(null);

        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSelectedAsync_WhenNothingSelected_ShouldDoNothing()
    {
        var vm = CreateVm(out var service, out _);

        await service.CreateAsync();
        await vm.LoadAsync();

        vm.SelectedNote = null;
        await vm.DeleteSelectedCommand.ExecuteAsync(null);

        vm.Notes.Should().HaveCount(1);
    }

    [Fact]
    public async Task SettingSelectedNote_ShouldSendNoteSelectedMessage()
    {
        var vm = CreateVm(out var service, out var spy);

        await service.CreateAsync();
        await vm.LoadAsync();

        vm.SelectedNote = vm.Notes.First();

        spy.OfType<NoteSelectedMessage>().Should().HaveCount(1);
    }

    [Fact]
    public async Task OnNoteDeleted_ShouldRemoveItemFromList()
    {
        var vm = CreateVm(out var service, out var spy);

        var note = await service.CreateAsync();
        await vm.LoadAsync();

        spy.Messenger.Send(new NoteDeletedMessage(note.Id));

        vm.Notes.Should().BeEmpty();
    }

    [Fact]
    public async Task OnNoteSaved_ShouldRefreshItem()
    {
        var vm = CreateVm(out var service, out var spy);

        var note = await service.CreateAsync("Original");
        await vm.LoadAsync();

        note.Title = "Renamed";
        spy.Messenger.Send(new NoteSavedMessage(note));

        vm.Notes.First().Title.Should().Be("Renamed");
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterNotes()
    {
        var vm = CreateVm(out var service, out _);

        await service.CreateAsync("Apple");
        await service.CreateAsync("Banana");

        vm.SearchQuery = "apple";

        await Task.Delay(50);
        await vm.LoadAsync();

        vm.Notes.Should().HaveCount(1);
        vm.Notes.First().Title.Should().Be("Apple");
    }
}