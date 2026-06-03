using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PagesManager.Core.Helpers;
using PagesManager.Core.Models;

namespace PagesManager.ViewModels;

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
    private string _updatedAtDisplay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PinText))]
    private bool _isPinned;

    public string PinText => IsPinned ? "📌 " : string.Empty;

    public NoteListItemViewModel(Note note)
    {
        Model = note;
        _title = string.IsNullOrWhiteSpace(note.Title) ? "Без названия" : note.Title;
        _preview = BuildPreview(note.Content);
        _updatedAt = note.UpdatedAt;
        _updatedAtDisplay = DateFormatter.FormatRelative(note.UpdatedAt, DateTime.UtcNow);
        _isPinned = note.IsPinned;
    }

    public void Refresh()
    {
        Title = string.IsNullOrWhiteSpace(Model.Title) ? "Без названия" : Model.Title;
        Preview = BuildPreview(Model.Content);
        UpdatedAt = Model.UpdatedAt;
        UpdatedAtDisplay = DateFormatter.FormatRelative(Model.UpdatedAt, DateTime.UtcNow);
        IsPinned = Model.IsPinned;
    }

    private static string BuildPreview(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "Нет дополнительного текста";
        var trimmed = content.Trim().Replace("\r", " ").Replace("\n", " ");
        return trimmed.Length > 60 ? trimmed[..60] + "…" : trimmed;
    }
}