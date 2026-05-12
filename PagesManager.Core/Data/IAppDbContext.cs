using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PagesManager.Core.Models;

namespace PagesManager.Core.Data;

public interface IAppDbContext
{
    DbSet<Note> Notes { get; }
    DbSet<Attachment> Attachments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}