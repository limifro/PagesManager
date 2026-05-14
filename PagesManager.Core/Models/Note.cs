using System;
using System.Collections.Generic;

namespace PagesManager.Core.Models;

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "Inter";
    public double FontSize { get; set; } = 14;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string TextAlignment { get; set; } = "Left";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPinned { get; set; }
    public List<Attachment> Attachments { get; set; } = new();
}