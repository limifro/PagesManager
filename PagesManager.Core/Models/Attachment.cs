using System;

namespace PagesManager.Core.Models;

public class Attachment
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public Note? Note { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}