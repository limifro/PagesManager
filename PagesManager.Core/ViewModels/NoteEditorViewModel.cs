using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PagesManager.Core.Messages;
using PagesManager.Core.Models;
using PagesManager.Core.Services;

namespace PagesManager.Core.ViewModels;

public partial class NoteEditorViewModel : ViewModelBase
{
    private readonly INoteService _noteService;
    private readonly IFilePickerService _filePicker;
    private readonly IMessenger _messenger;

    private Note? _currentNote;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private double _fontSize = 14;

    [ObservableProperty]
    private string _fontFamily = "Inter";

    [ObservableProperty]
    private bool _hasNote;

    public ObservableCollection<AttachmentViewModel> Attachments { get; } = new();

    public NoteEditorViewModel(
        INoteService noteService,
        IFilePickerService filePicker,
        IMessenger messenger)
    {
        _noteService = noteService;
        _filePicker = filePicker;
        _messenger = messenger;
    }

    public Note? CurrentNote => _currentNote;

    public void Load(Note note)
    {
        _currentNote = note ?? throw new ArgumentNullException(nameof(note));
        Title = note.Title;
        Content = note.Content;
        FontSize = note.FontSize;
        FontFamily = note.FontFamily;
        HasNote = true;

        Attachments.Clear();
        foreach (var att in note.Attachments)
            Attachments.Add(new AttachmentViewModel(att));
    }

    public void Clear()
    {
        _currentNote = null;
        Title = string.Empty;
        Content = string.Empty;
        FontSize = 14;
        FontFamily = "Inter";
        HasNote = false;
        Attachments.Clear();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_currentNote is null) return;

        _currentNote.Title = string.IsNullOrWhiteSpace(Title) ? "Без названия" : Title.Trim();
        _currentNote.Content = Content ?? string.Empty;
        _currentNote.FontSize = FontSize;
        _currentNote.FontFamily = FontFamily;

        await _noteService.UpdateAsync(_currentNote);
        _messenger.Send(new NoteSavedMessage(_currentNote));
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_currentNote is null) return;

        var id = _currentNote.Id;
        await _noteService.DeleteAsync(id);
        _messenger.Send(new NoteDeletedMessage(id));
        Clear();
    }

    [RelayCommand]
    private async Task TogglePinAsync()
    {
        if (_currentNote is null) return;

        await _noteService.TogglePinAsync(_currentNote.Id);

        var updated = await _noteService.GetByIdAsync(_currentNote.Id);
        if (updated is not null)
        {
            _currentNote = updated;
            _messenger.Send(new NoteSavedMessage(_currentNote));
        }
    }

    [RelayCommand]
    private async Task AttachImageAsync()
    {
        if (_currentNote is null) return;

        var picked = await _filePicker.PickImagesAsync(allowMultiple: true);
        if (picked.Count == 0) return;

        foreach (var file in picked)
        {
            await using var stream = file.OpenRead;
            var attachment = await _noteService.AttachFileAsync(
                _currentNote.Id,
                stream,
                file.FileName,
                file.ContentType);

            _currentNote.Attachments.Add(attachment);
            Attachments.Add(new AttachmentViewModel(attachment));
        }

        _messenger.Send(new NoteSavedMessage(_currentNote));
    }

    [RelayCommand]
    private async Task RemoveAttachmentAsync(AttachmentViewModel? attachment)
    {
        if (attachment is null || _currentNote is null) return;

        await _noteService.RemoveAttachmentAsync(attachment.Id);
        Attachments.Remove(attachment);
        _currentNote.Attachments.RemoveAll(a => a.Id == attachment.Id);
        _messenger.Send(new NoteSavedMessage(_currentNote));
    }
}