using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PagesManager.Core.Data;
using PagesManager.Core.Helpers;
using PagesManager.Core.Models;

namespace PagesManager.Core.Services;

public class NoteService : INoteService
{
    private readonly IFileStorageService _fileStorage;
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public NoteService(
        IFileStorageService fileStorage,
        IAppDbContext db,
        IClock clock)
    {
        _fileStorage = fileStorage;
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Notes
            .Include(n => n.Attachments)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Note?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Notes
            .Include(n => n.Attachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<Note> CreateAsync(string title = "Новая заметка", string content = "", CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var note = new Note
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Новая заметка" : title.Trim(),
            Content = content ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync(ct);
        return note;
    }

    public async Task UpdateAsync(Note note, CancellationToken ct = default)
    {
        if (note is null) throw new ArgumentNullException(nameof(note));

        var tracked = await _db.Notes.FirstOrDefaultAsync(n => n.Id == note.Id, ct);
        if (tracked is null) return;

        tracked.Title = note.Title;
        tracked.Content = note.Content;
        tracked.FontFamily = note.FontFamily;
        tracked.FontSize = note.FontSize;
        tracked.IsBold = note.IsBold;
        tracked.IsItalic = note.IsItalic;
        tracked.IsUnderline = note.IsUnderline;
        tracked.TextAlignment = note.TextAlignment;
        tracked.IsPinned = note.IsPinned;
        tracked.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var note = await _db.Notes
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
        if (note is null) return;

        foreach (var att in note.Attachments)
            _fileStorage.Delete(att.FilePath);

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Note>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync(ct);

        var q = query.Trim();

        var all = await _db.Notes
            .Include(n => n.Attachments)
            .AsNoTracking()
            .ToListAsync(ct);

        return all
            .Where(n => (n.Title?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                     || (n.Content?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToList();
    }

    public async Task TogglePinAsync(int id, CancellationToken ct = default)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (note is null) return;

        note.IsPinned = !note.IsPinned;
        note.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Attachment> AttachFileAsync(int noteId, Stream stream, string originalFileName, string contentType, CancellationToken ct = default)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == noteId, ct)
                   ?? throw new InvalidOperationException($"Note {noteId} not found");

        var savedPath = await _fileStorage.SaveAsync(stream, originalFileName, ct);

        var attachment = new Attachment
        {
            NoteId = noteId,
            FilePath = savedPath,
            FileName = originalFileName,
            ContentType = contentType ?? string.Empty,
            AddedAt = _clock.UtcNow
        };

        _db.Attachments.Add(attachment);
        note.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        return attachment;
    }

    public async Task<Attachment> AddExistingAttachmentAsync(
        int noteId,
        string filePath,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));

        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == noteId, ct)
                   ?? throw new InvalidOperationException($"Note {noteId} not found");

        var attachment = new Attachment
        {
            NoteId = noteId,
            FilePath = filePath,
            FileName = fileName ?? string.Empty,
            ContentType = contentType ?? string.Empty,
            AddedAt = _clock.UtcNow
        };

        _db.Attachments.Add(attachment);
        note.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        return attachment;
    }

    public async Task RemoveAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var att = await _db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (att is null) return;

        _fileStorage.Delete(att.FilePath);
        _db.Attachments.Remove(att);
        await _db.SaveChangesAsync(ct);
    }
}