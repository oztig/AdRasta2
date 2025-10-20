using System.Threading.Tasks;
using AdRasta2.ViewModels.Helpers;
using Avalonia.Platform.Storage;

namespace AdRasta2.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(FileBrowseRequest request);
}

