using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PagesManager.Core.Models;

namespace PagesManager.Core.Services;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct = default);
    Task<Note?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Note> AddAsync(Note note, CancellationToken ct = default);
    Task UpdateAsync(Note note, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Note>> SearchAsync(string query, CancellationToken ct = default);
}