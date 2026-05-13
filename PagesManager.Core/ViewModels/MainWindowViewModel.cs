using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PagesManager.Core.Messages;

namespace PagesManager.Core.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    public NoteListViewModel NoteList { get; }
    public NoteEditorViewModel NoteEditor { get; }

    public MainWindowViewModel(
        NoteListViewModel noteList,
        NoteEditorViewModel noteEditor,
        IMessenger messenger)
    {
        NoteList = noteList;
        NoteEditor = noteEditor;
        _messenger = messenger;

        _messenger.Register<NoteSelectedMessage>(this, (_, m) => NoteEditor.Load(m.Note));
        _messenger.Register<NoteDeletedMessage>(this, (_, _) =>
        {
            if (NoteEditor.CurrentNote is null)
                NoteEditor.Clear();
        });
    }

    public Task InitializeAsync() => NoteList.LoadAsync();
}