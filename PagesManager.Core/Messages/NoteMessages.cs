using PagesManager.Core.Models;

namespace PagesManager.Core.Messages;

public record NoteSelectedMessage(Note Note);
public record NoteSavedMessage(Note Note);
public record NoteDeletedMessage(int NoteId);
public record CreateNoteRequestedMessage();