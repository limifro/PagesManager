using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PagesManager.Core.Models;
using PagesManager.Core.Services;
using PagesManager.Tests.Helpers;

namespace PagesManager.Tests.Services;

public class NoteServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateNoteWithDefaultValues()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();

        note.Id.Should().BeGreaterThan(0);
        note.Title.Should().Be("Новая заметка");
        note.Content.Should().BeEmpty();
        note.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        note.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var fromDb = await db.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsWhitespace_ShouldUseDefaultTitle()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync("   ");

        note.Title.Should().Be("Новая заметка");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateNoteFields()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();

        note.Title = "Updated";
        note.Content = "Updated content";
        note.FontFamily = "Georgia";
        note.FontSize = 20;
        note.IsBold = true;
        note.IsItalic = true;
        note.TextAlignment = "Center";

        await service.UpdateAsync(note);

        var updated = await db.Notes.FirstAsync(n => n.Id == note.Id);
        updated.Title.Should().Be("Updated");
        updated.Content.Should().Be("Updated content");
        updated.FontFamily.Should().Be("Georgia");
        updated.FontSize.Should().Be(20);
        updated.IsBold.Should().BeTrue();
        updated.IsItalic.Should().BeTrue();
        updated.TextAlignment.Should().Be("Center");
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TogglePinAsync_ShouldInvertPinState()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();
        note.IsPinned.Should().BeFalse();

        await service.TogglePinAsync(note.Id);
        var pinned = await db.Notes.FirstAsync(n => n.Id == note.Id);
        pinned.IsPinned.Should().BeTrue();

        await service.TogglePinAsync(note.Id);
        var unpinned = await db.Notes.FirstAsync(n => n.Id == note.Id);
        unpinned.IsPinned.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPinnedFirstThenUpdatedDescending()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note1 = await service.CreateAsync("Old");
        await Task.Delay(10);
        var note2 = await service.CreateAsync("New");
        await Task.Delay(10);
        var note3 = await service.CreateAsync("Pinned");

        await service.TogglePinAsync(note1.Id);

        var notes = await service.GetAllAsync();
        notes.First().Id.Should().Be(note1.Id);
        notes.Should().Contain(n => n.Id == note2.Id);
        notes.Should().Contain(n => n.Id == note3.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByTitleIgnoringCase()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        await service.CreateAsync("Shopping list", "milk");
        await service.CreateAsync("Work", "meeting");

        var result = await service.SearchAsync("shopping");

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Shopping list");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByContentIgnoringCase()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        await service.CreateAsync("First", "Buy milk");
        await service.CreateAsync("Second", "Meeting");

        var result = await service.SearchAsync("milk");

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("First");
    }

    [Fact]
    public async Task AttachFileAsync_ShouldSaveFileAndCreateAttachment()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var attachment = await service.AttachFileAsync(note.Id, stream, "image.png", "image/png");

        attachment.Id.Should().BeGreaterThan(0);
        attachment.NoteId.Should().Be(note.Id);
        attachment.FileName.Should().Be("image.png");
        attachment.ContentType.Should().Be("image/png");
        storage.Files.Should().ContainKey(attachment.FilePath);

        var fromDb = await db.Attachments.FirstOrDefaultAsync(a => a.Id == attachment.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task AddExistingAttachmentAsync_ShouldCreateAttachmentWithoutSavingFileAgain()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();

        var attachment = await service.AddExistingAttachmentAsync(
            note.Id, "/existing/file.png", "file.png", "image/png");

        attachment.Id.Should().BeGreaterThan(0);
        attachment.FilePath.Should().Be("/existing/file.png");
        storage.Files.Should().BeEmpty();

        var fromDb = await db.Attachments.FirstOrDefaultAsync(a => a.Id == attachment.Id);
        fromDb.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveAttachmentAsync_ShouldDeleteAttachmentAndFile()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();
        var attachment = await service.AddExistingAttachmentAsync(
            note.Id, "/fake/storage/photo.png", "photo.png", "image/png");

        await service.RemoveAttachmentAsync(attachment.Id);

        var fromDb = await db.Attachments.FirstOrDefaultAsync(a => a.Id == attachment.Id);
        fromDb.Should().BeNull();
        storage.DeletedFiles.Should().Contain("/fake/storage/photo.png");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteNoteAndAttachmentFiles()
    {
        var db = TestDbContextFactory.Create();
        var storage = new FakeFileStorageService();
        var service = new NoteService(storage, db);

        var note = await service.CreateAsync();
        await service.AddExistingAttachmentAsync(
            note.Id, "/fake/storage/photo.png", "photo.png", "image/png");

        await service.DeleteAsync(note.Id);

        var noteFromDb = await db.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
        noteFromDb.Should().BeNull();
        storage.DeletedFiles.Should().Contain("/fake/storage/photo.png");
    }
}