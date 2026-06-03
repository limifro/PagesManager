using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PagesManager.Core.Messages;
using PagesManager.Core.Models;
using PagesManager.Core.Services;
using PagesManager.ViewModels;
using PagesManager.Tests.Helpers;

namespace PagesManager.Tests.ViewModels;

public class NoteEditorViewModelTests
{
    private static NoteEditorViewModel CreateVm(
        out NoteService service,
        out FakeFileStorageService storage,
        out FakeFilePickerService picker,
        out FakeImagePreviewService preview,
        out MessengerSpy spy)
    {
        var db = TestDbContextFactory.Create();
        storage = new FakeFileStorageService();
        service = new NoteService(storage, db);
        picker = new FakeFilePickerService();
        preview = new FakeImagePreviewService();
        spy = new MessengerSpy();
        spy.RegisterAll<NoteSavedMessage>();
        spy.RegisterAll<NoteDeletedMessage>();
        return new NoteEditorViewModel(service, picker, preview, storage, spy.Messenger);
    }

    [Fact]
    public void Load_ShouldPopulateFieldsFromNote()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        var note = new Note
        {
            Id = 1,
            Title = "Title",
            Content = "Body",
            FontFamily = "Georgia",
            FontSize = 18,
            IsBold = true,
            IsItalic = true,
            TextAlignment = "Center",
            IsPinned = true
        };

        vm.Load(note);

        vm.HasNote.Should().BeTrue();
        vm.Title.Should().Be("Title");
        vm.Content.Should().Be("Body");
        vm.FontFamily.Should().Be("Georgia");
        vm.FontSize.Should().Be(18);
        vm.IsBold.Should().BeTrue();
        vm.IsItalic.Should().BeTrue();
        vm.TextAlignment.Should().Be("Center");
        vm.IsPinned.Should().BeTrue();
        vm.CurrentNote.Should().BeSameAs(note);
    }

    [Fact]
    public void Load_WhenNoteIsNull_ShouldThrow()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);

        Action act = () => vm.Load(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Clear_ShouldResetAllFields()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "X", Content = "Y" });

        vm.Clear();

        vm.HasNote.Should().BeFalse();
        vm.CurrentNote.Should().BeNull();
        vm.Title.Should().BeEmpty();
        vm.Content.Should().BeEmpty();
        vm.IsBold.Should().BeFalse();
        vm.IsItalic.Should().BeFalse();
        vm.IsPinned.Should().BeFalse();
        vm.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ShouldUpdateNoteInDb()
    {
        var vm = CreateVm(out var service, out _, out _, out _, out var spy);
        var note = await service.CreateAsync();
        vm.Load(note);

        vm.Title = "Updated";
        vm.Content = "Updated body";
        vm.FontSize = 22;
        vm.IsBold = true;

        await vm.SaveCommand.ExecuteAsync(null);

        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb!.Title.Should().Be("Updated");
        fromDb.Content.Should().Be("Updated body");
        fromDb.FontSize.Should().Be(22);
        fromDb.IsBold.Should().BeTrue();
        spy.OfType<NoteSavedMessage>().Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_WhenTitleIsEmpty_ShouldUseDefaultTitle()
    {
        var vm = CreateVm(out var service, out _, out _, out _, out _);
        var note = await service.CreateAsync();
        vm.Load(note);

        vm.Title = "   ";
        await vm.SaveCommand.ExecuteAsync(null);

        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb!.Title.Should().Be("Без названия");
    }

    [Fact]
    public async Task SaveAsync_WhenNoNoteLoaded_ShouldDoNothing()
    {
        var vm = CreateVm(out _, out _, out _, out _, out var spy);

        await vm.SaveCommand.ExecuteAsync(null);

        spy.OfType<NoteSavedMessage>().Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveNoteAndSendMessage()
    {
        var vm = CreateVm(out var service, out _, out _, out _, out var spy);
        var note = await service.CreateAsync();
        vm.Load(note);

        await vm.DeleteCommand.ExecuteAsync(null);

        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb.Should().BeNull();
        vm.HasNote.Should().BeFalse();
        spy.OfType<NoteDeletedMessage>().Should().HaveCount(1);
    }

    [Fact]
    public async Task TogglePinAsync_ShouldFlipPinAndNotify()
    {
        var vm = CreateVm(out var service, out _, out _, out _, out var spy);
        var note = await service.CreateAsync();
        vm.Load(note);

        vm.IsPinned.Should().BeFalse();

        await vm.TogglePinCommand.ExecuteAsync(null);

        vm.IsPinned.Should().BeTrue();
        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb!.IsPinned.Should().BeTrue();
        spy.OfType<NoteSavedMessage>().Should().HaveCount(1);
    }

    [Fact]
    public void ToggleBold_ShouldFlipIsBold()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        vm.IsBold.Should().BeFalse();
        vm.ToggleBoldCommand.Execute(null);
        vm.IsBold.Should().BeTrue();
        vm.ToggleBoldCommand.Execute(null);
        vm.IsBold.Should().BeFalse();
    }

    [Fact]
    public void ToggleItalic_ShouldFlipIsItalic()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        vm.ToggleItalicCommand.Execute(null);
        vm.IsItalic.Should().BeTrue();
    }

    [Fact]
    public void ToggleUnderline_ShouldFlipIsUnderline()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        vm.ToggleUnderlineCommand.Execute(null);
        vm.IsUnderline.Should().BeTrue();
    }

    [Fact]
    public void AlignCommands_ShouldUpdateTextAlignment()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        vm.AlignCenterCommand.Execute(null);
        vm.TextAlignment.Should().Be("Center");

        vm.AlignRightCommand.Execute(null);
        vm.TextAlignment.Should().Be("Right");

        vm.AlignLeftCommand.Execute(null);
        vm.TextAlignment.Should().Be("Left");
    }

    [Fact]
    public async Task AttachImageAsync_ShouldAddPendingAttachment()
    {
        var vm = CreateVm(out var service, out var storage, out var picker, out _, out _);
        var note = await service.CreateAsync();
        vm.Load(note);

        picker.NextResult = new List<PickedFile>
        {
            new("photo.png", "image/png", new MemoryStream(new byte[] { 1, 2, 3 }))
        };

        await vm.AttachImageCommand.ExecuteAsync(null);

        vm.Attachments.Should().HaveCount(1);
        storage.Files.Should().HaveCount(1);
        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb!.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task AttachImageAsync_ThenSave_ShouldPersistAttachment()
    {
        var vm = CreateVm(out var service, out _, out var picker, out _, out _);
        var note = await service.CreateAsync();
        vm.Load(note);

        picker.NextResult = new List<PickedFile>
        {
            new("photo.png", "image/png", new MemoryStream(new byte[] { 1, 2, 3 }))
        };

        await vm.AttachImageCommand.ExecuteAsync(null);
        await vm.SaveCommand.ExecuteAsync(null);

        var fromDb = await service.GetByIdAsync(note.Id);
        fromDb!.Attachments.Should().HaveCount(1);
        fromDb.Attachments[0].FileName.Should().Be("photo.png");
    }

    [Fact]
    public async Task AttachImageAsync_ThenLoadAnotherNote_ShouldDiscardPendingFiles()
    {
        var vm = CreateVm(out var service, out var storage, out var picker, out _, out _);
        var note1 = await service.CreateAsync("First");
        var note2 = await service.CreateAsync("Second");
        vm.Load(note1);

        picker.NextResult = new List<PickedFile>
        {
            new("photo.png", "image/png", new MemoryStream(new byte[] { 1, 2, 3 }))
        };

        await vm.AttachImageCommand.ExecuteAsync(null);
        storage.Files.Should().HaveCount(1);

        vm.Load(note2);
        storage.DeletedFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveAttachmentAsync_ForPending_ShouldDeleteFileImmediately()
    {
        var vm = CreateVm(out var service, out var storage, out var picker, out _, out _);
        var note = await service.CreateAsync();
        vm.Load(note);

        picker.NextResult = new List<PickedFile>
        {
            new("photo.png", "image/png", new MemoryStream(new byte[] { 1, 2, 3 }))
        };

        await vm.AttachImageCommand.ExecuteAsync(null);
        var pendingAtt = vm.Attachments.First();

        await vm.RemoveAttachmentCommand.ExecuteAsync(pendingAtt);

        vm.Attachments.Should().BeEmpty();
        storage.DeletedFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveAttachmentAsync_ForExisting_ShouldMarkForDeletionOnSave()
    {
        var vm = CreateVm(out var service, out var storage, out _, out _, out _);
        var note = await service.CreateAsync();
        await service.AddExistingAttachmentAsync(note.Id, "/fake/storage/x.png", "x.png", "image/png");
        var refreshed = await service.GetByIdAsync(note.Id);
        vm.Load(refreshed!);

        var attVm = vm.Attachments.First();
        await vm.RemoveAttachmentCommand.ExecuteAsync(attVm);

        storage.DeletedFiles.Should().BeEmpty();

        await vm.SaveCommand.ExecuteAsync(null);

        var afterSave = await service.GetByIdAsync(note.Id);
        afterSave!.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAttachmentAsync_WhenAttachmentIsNull_ShouldDoNothing()
    {
        var vm = CreateVm(out _, out _, out _, out _, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        await vm.RemoveAttachmentCommand.ExecuteAsync(null);

        vm.Attachments.Should().BeEmpty();
    }

    [Fact]
    public async Task OpenAttachmentPreviewAsync_ShouldCallPreviewService()
    {
        var vm = CreateVm(out _, out _, out _, out var preview, out _);
        vm.Load(new Note { Id = 1, Title = "x" });

        var att = new AttachmentViewModel(new Attachment
        {
            Id = 1,
            FilePath = "/path/img.png",
            FileName = "img.png"
        });

        await vm.OpenAttachmentPreviewCommand.ExecuteAsync(att);

        preview.Shown.Should().HaveCount(1);
        preview.Shown[0].Path.Should().Be("/path/img.png");
        preview.Shown[0].Title.Should().Be("img.png");
    }

    [Fact]
    public async Task OpenAttachmentPreviewAsync_WhenAttachmentIsNull_ShouldDoNothing()
    {
        var vm = CreateVm(out _, out _, out _, out var preview, out _);

        await vm.OpenAttachmentPreviewCommand.ExecuteAsync(null);

        preview.Shown.Should().BeEmpty();
    }
}