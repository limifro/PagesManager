using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IImagePreviewService _imagePreviewService;
    private readonly IFileStorageService _fileStorage;
    private readonly IMessenger _messenger;

    private Note? _currentNote;

    private readonly HashSet<int> _pendingDeletedAttachmentIds = new();
    private readonly List<PendingAttachment> _pendingAddedAttachments = new();

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private double _fontSize = 14;
    [ObservableProperty] private string _fontFamily = "Inter";
    [ObservableProperty] private bool _hasNote;

    public ObservableCollection<AttachmentViewModel> Attachments { get; } = new();

    public NoteEditorViewModel(
        INoteService noteService,
        IFilePickerService filePicker,
        IImagePreviewService imagePreviewService,
        IFileStorageService fileStorage,
        IMessenger messenger)
    {
        _noteService = noteService;
        _filePicker = filePicker;
        _imagePreviewService = imagePreviewService;
        _fileStorage = fileStorage;
        _messenger = messenger;
    }

    public Note? CurrentNote => _currentNote;

    public void Load(Note note)
    {
        if (note is null) throw new ArgumentNullException(nameof(note));

        DiscardPendingChanges();

        _currentNote = note;

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
        DiscardPendingChanges();

        _currentNote = null;

        Title = string.Empty;
        Content = string.Empty;
        FontSize = 14;
        FontFamily = "Inter";
        HasNote = false;
        Attachments.Clear();
    }

    private void DiscardPendingChanges()
    {
        foreach (var pending in _pendingAddedAttachments)
        {
            try { _fileStorage.Delete(pending.FilePath); }
            catch
            {
                
            }
        }
        _pendingAddedAttachments.Clear();
        _pendingDeletedAttachmentIds.Clear();
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

        foreach (var attachmentId in _pendingDeletedAttachmentIds.ToList())
            await _noteService.RemoveAttachmentAsync(attachmentId);
        _pendingDeletedAttachmentIds.Clear();

        foreach (var pending in _pendingAddedAttachments.ToList())
        {
            await _noteService.AddExistingAttachmentAsync(
                _currentNote.Id,
                pending.FilePath,
                pending.FileName,
                pending.ContentType);
        }
        _pendingAddedAttachments.Clear();

        var refreshed = await _noteService.GetByIdAsync(_currentNote.Id);
        if (refreshed is not null)
        {
            Load(refreshed);
            _messenger.Send(new NoteSavedMessage(refreshed));
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_currentNote is null) return;

        DiscardPendingChanges();

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
        _currentNote.IsPinned = !_currentNote.IsPinned;
        _messenger.Send(new NoteSavedMessage(_currentNote));
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

            var savedPath = await _fileStorage.SaveAsync(stream, file.FileName);

            var pendingModel = new Attachment
            {
                Id = 0,
                NoteId = _currentNote.Id,
                FilePath = savedPath,
                FileName = file.FileName,
                ContentType = file.ContentType,
                AddedAt = DateTime.UtcNow
            };

            var vm = new AttachmentViewModel(pendingModel);
            Attachments.Add(vm);
            _pendingAddedAttachments.Add(new PendingAttachment(
                savedPath, file.FileName, file.ContentType, vm));
        }
    }

    [RelayCommand]
    private Task RemoveAttachmentAsync(AttachmentViewModel? attachment)
    {
        if (attachment is null || _currentNote is null)
            return Task.CompletedTask;

        var pending = _pendingAddedAttachments.FirstOrDefault(p => p.ViewModel == attachment);
        if (pending is not null)
        {
            try { _fileStorage.Delete(pending.FilePath); }
            catch
            {
                
            }

            _pendingAddedAttachments.Remove(pending);
            Attachments.Remove(attachment);
            return Task.CompletedTask;
        }

        _pendingDeletedAttachmentIds.Add(attachment.Id);
        Attachments.Remove(attachment);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenAttachmentPreviewAsync(AttachmentViewModel? attachment)
    {
        if (attachment is null) return;
        await _imagePreviewService.ShowAsync(attachment.FilePath, attachment.FileName);
    }

    private sealed record PendingAttachment(
        string FilePath,
        string FileName,
        string ContentType,
        AttachmentViewModel ViewModel);
}