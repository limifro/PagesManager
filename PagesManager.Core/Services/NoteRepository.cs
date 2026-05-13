using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PagesManager.Core.Data;
using PagesManager.Core.Models;

namespace PagesManager.Core.Services;

public class NoteRepository : INoteRepository
{
    private readonly IAppDbContext _db;

    public NoteRepository(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Notes
            .Include(n => n.Attachments)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Note?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Notes
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<Note> AddAsync(Note note, CancellationToken ct = default)
    {
        if (note is null) throw new ArgumentNullException(nameof(note));

        _db.Notes.Add(note);
        await _db.SaveChangesAsync(ct);
        return note;
    }

    public async Task UpdateAsync(Note note, CancellationToken ct = default)
    {
    if (note is null) throw new ArgumentNullException(nameof(note));

    var tracked = await _db.Notes.FirstOrDefaultAsync(n => n.Id == note.Id, ct);
    if (tracked is null)
    {
        return;
    }

    tracked.Title = note.Title;
    tracked.Content = note.Content;
    tracked.FontFamily = note.FontFamily;
    tracked.FontSize = note.FontSize;
    tracked.IsPinned = note.IsPinned;
    tracked.UpdatedAt = note.UpdatedAt;

    await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (note is null) return;

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
        .ToListAsync(ct);

    return all
        .Where(n => (n.Title?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                 || (n.Content?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
        .OrderByDescending(n => n.IsPinned)
        .ThenByDescending(n => n.UpdatedAt)
        .ToList();
    }
}