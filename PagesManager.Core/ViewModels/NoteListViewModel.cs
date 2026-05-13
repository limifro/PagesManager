using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PagesManager.Core.Messages;
using PagesManager.Core.Services;

namespace PagesManager.Core.ViewModels;

public partial class NoteListViewModel : ViewModelBase
{
    private readonly INoteService _noteService;
    private readonly IMessenger _messenger;
    private CancellationTokenSource? _searchCts;

    public ObservableCollection<NoteListItemViewModel> Notes { get; } = new();

    [ObservableProperty]
    private NoteListItemViewModel? _selectedNote;

    [ObservableProperty]
    private string _searchQuery = string.Empty;
    public int SearchDebounceMs { get; set; } = 150;

    public NoteListViewModel(INoteService noteService, IMessenger messenger)
    {
        _noteService = noteService;
        _messenger = messenger;

        _messenger.Register<NoteSavedMessage>(this, (_, m) => OnNoteSaved(m));
        _messenger.Register<NoteDeletedMessage>(this, (_, m) => OnNoteDeleted(m));
    }

    partial void OnSelectedNoteChanged(NoteListItemViewModel? value)
    {
        if (value is not null)
            _messenger.Send(new NoteSelectedMessage(value.Model));
    }

    partial void OnSearchQueryChanged(string value)
    {
        _ = DebouncedSearchAsync();
    }

    private async Task DebouncedSearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(SearchDebounceMs, token);
            if (token.IsCancellationRequested) return;
            await LoadAsync(token);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        var items = string.IsNullOrWhiteSpace(SearchQuery)
            ? await _noteService.GetAllAsync(ct)
            : await _noteService.SearchAsync(SearchQuery, ct);

        if (ct.IsCancellationRequested) return;

        Notes.Clear();
        foreach (var n in items)
            Notes.Add(new NoteListItemViewModel(n));
    }

    [RelayCommand]
    private async Task CreateNoteAsync()
    {
        var note = await _noteService.CreateAsync();
        var vm = new NoteListItemViewModel(note);
        Notes.Insert(0, vm);
        SelectedNote = vm;
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedNote is null) return;
        await _noteService.DeleteAsync(SelectedNote.Model.Id);
    }

    private void OnNoteSaved(NoteSavedMessage m)
    {
        var existing = Notes.FirstOrDefault(n => n.Model.Id == m.Note.Id);
        if (existing is not null)
        {
            existing.Refresh();
            ResortNotes();
        }
    }

    private void OnNoteDeleted(NoteDeletedMessage m)
    {
        var existing = Notes.FirstOrDefault(n => n.Model.Id == m.NoteId);
        if (existing is not null)
            Notes.Remove(existing);

        if (SelectedNote == existing)
            SelectedNote = null;
    }

    private void ResortNotes()
    {
        var sorted = Notes
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            var currentIndex = Notes.IndexOf(sorted[i]);
            if (currentIndex != i)
                Notes.Move(currentIndex, i);
        }
    }
}