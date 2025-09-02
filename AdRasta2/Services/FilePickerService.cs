using System;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AdRasta2.Services;

public class FilePickerService : IFilePickerService
{
    private readonly Window _window;

    public FilePickerService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public async Task<string?> PickFileAsync(FilePickerFileType fileType, string title = "Select a file")
    {
        if (_window.StorageProvider is null)
            return null;

        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { fileType }
        });

        return files.Count > 0
            ? Uri.UnescapeDataString(files[0].Path.AbsolutePath)
            : null;
    }
}
