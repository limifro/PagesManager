using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PagesManager.Core.Models;

namespace PagesManager.Core.Services;

public interface INoteService
{
    Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct = default);
    Task<Note?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Note> CreateAsync(string title = "Новая заметка", string content = "", CancellationToken ct = default);
    Task UpdateAsync(Note note, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Note>> SearchAsync(string query, CancellationToken ct = default);
    Task TogglePinAsync(int id, CancellationToken ct = default);

    Task<Attachment> AttachFileAsync(int noteId, Stream stream, string originalFileName, string contentType, CancellationToken ct = default);
    Task RemoveAttachmentAsync(int attachmentId, CancellationToken ct = default);
}