using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PagesManager.Core.Messages;
using PagesManager.Core.Services;

namespace PagesManager.Core.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly IThemeService _themeService;

    public NoteListViewModel NoteList { get; }
    public NoteEditorViewModel NoteEditor { get; }

    [ObservableProperty]
    private bool _isDarkTheme;

    public MainWindowViewModel(
        NoteListViewModel noteList,
        NoteEditorViewModel noteEditor,
        IMessenger messenger,
        IThemeService themeService)
    {
        NoteList = noteList;
        NoteEditor = noteEditor;
        _messenger = messenger;
        _themeService = themeService;

        _messenger.Register<NoteSelectedMessage>(this, (_, m) => NoteEditor.Load(m.Note));
        _messenger.Register<NoteDeletedMessage>(this, (_, _) =>
        {
            if (NoteEditor.CurrentNote is null)
                NoteEditor.Clear();
        });

        IsDarkTheme = _themeService.CurrentTheme == AppTheme.Dark;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        IsDarkTheme = _themeService.CurrentTheme == AppTheme.Dark;
    }

    public Task InitializeAsync() => NoteList.LoadAsync();
}