using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PagesManager.Core.Data;
using PagesManager.Core.Helpers;
using PagesManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace PagesManager.Core.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public NoteService(
        INoteRepository repository,
        IFileStorageService fileStorage,
        IAppDbContext db,
        IClock clock)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _db = db;
        _clock = clock;
    }

    public Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    public Task<Note?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

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

        return await _repository.AddAsync(note, ct);
    }

    public async Task UpdateAsync(Note note, CancellationToken ct = default)
    {
        if (note is null) throw new ArgumentNullException(nameof(note));

        note.UpdatedAt = _clock.UtcNow;
        await _repository.UpdateAsync(note, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(id, ct);
        if (note is null) return;

        // Удаляем файлы вложений с диска
        foreach (var att in note.Attachments)
            _fileStorage.Delete(att.FilePath);

        await _repository.DeleteAsync(id, ct);
    }

    public Task<IReadOnlyList<Note>> SearchAsync(string query, CancellationToken ct = default)
        => _repository.SearchAsync(query, ct);

    public async Task TogglePinAsync(int id, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(id, ct);
        if (note is null) return;

        note.IsPinned = !note.IsPinned;
        note.UpdatedAt = _clock.UtcNow;
        await _repository.UpdateAsync(note, ct);
    }

    public async Task<Attachment> AttachFileAsync(int noteId, Stream stream, string originalFileName, string contentType, CancellationToken ct = default)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        var note = await _repository.GetByIdAsync(noteId, ct)
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

    public async Task RemoveAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var att = await _db.Attachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (att is null) return;

        _fileStorage.Delete(att.FilePath);
        _db.Attachments.Remove(att);
        await _db.SaveChangesAsync(ct);
    }
}