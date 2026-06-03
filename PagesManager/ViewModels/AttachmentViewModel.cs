using CommunityToolkit.Mvvm.ComponentModel;
using PagesManager.Core.Models;

namespace PagesManager.ViewModels;

public partial class AttachmentViewModel : ViewModelBase
{
    public Attachment Model { get; }

    [ObservableProperty]
    private string _fileName;

    [ObservableProperty]
    private string _filePath;

    public int Id => Model.Id;

    public AttachmentViewModel(Attachment model)
    {
        Model = model;
        _fileName = model.FileName;
        _filePath = model.FilePath;
    }
}