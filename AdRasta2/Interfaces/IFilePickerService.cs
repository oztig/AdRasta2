using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace AdRasta2.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(FilePickerFileType fileType, string title = "Select a file");
}
