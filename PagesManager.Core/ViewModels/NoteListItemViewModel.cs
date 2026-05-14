using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PagesManager.Core.Models;

namespace PagesManager.Core.ViewModels;

public partial class NoteListItemViewModel : ViewModelBase
{
    public Note Model { get; set; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _preview;

    [ObservableProperty]
    private DateTime _updatedAt;

    [ObservableProperty]
    private bool _isPinned;

    public NoteListItemViewModel(Note note)
    {
        Model = note;
        _title = note.Title;
        _preview = BuildPreview(note.Content);
        _updatedAt = note.UpdatedAt;
        _isPinned = note.IsPinned;
    }

    public void Refresh()
    {
        Title = Model.Title;
        Preview = BuildPreview(Model.Content);
        UpdatedAt = Model.UpdatedAt;
        IsPinned = Model.IsPinned;
    }

    private static string BuildPreview(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        var trimmed = content.Trim().Replace("\r", " ").Replace("\n", " ");
        return trimmed.Length > 80 ? trimmed[..80] + "…" : trimmed;
    }
}