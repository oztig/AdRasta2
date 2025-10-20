using Avalonia.Platform.Storage;

namespace AdRasta2.ViewModels.Helpers;

public class FileBrowseRequest
{
    public string TargetProperty { get; set; } = "";
    public FilePickerFileType FileType { get; set; } = new("All") { Patterns = new[] { "*" } };
}

