using PagesManager.Core.Models;

namespace PagesManager.Core.Messages;

public class NoteSelectedMessage
{
    public Note Note { get; }
    public NoteSelectedMessage(Note note) { Note = note; }
}

public class NoteSavedMessage
{
    public Note Note { get; }
    public NoteSavedMessage(Note note) { Note = note; }
}

public class NoteDeletedMessage
{
    public int NoteId { get; }
    public NoteDeletedMessage(int noteId) { NoteId = noteId; }
}

public class CreateNoteRequestedMessage
{
    
}