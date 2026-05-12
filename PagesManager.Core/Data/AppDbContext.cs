using Microsoft.EntityFrameworkCore;
using PagesManager.Core.Models;

namespace PagesManager.Core.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).HasMaxLength(255).IsRequired();
            entity.Property(n => n.Content).IsRequired();
            entity.Property(n => n.FontFamily).HasMaxLength(100);

            entity.HasMany(n => n.Attachments)
                  .WithOne(a => a.Note!)
                  .HasForeignKey(a => a.NoteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FilePath).IsRequired();
            entity.Property(a => a.FileName).HasMaxLength(255);
            entity.Property(a => a.ContentType).HasMaxLength(100);
        });
    }
}