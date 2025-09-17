using System;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AdRasta2.Services;

public class FolderPickerService : IFolderPickerService
{
    private readonly Window _window;

    public FolderPickerService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public async Task<string> PickFolderAsync(string title = "Select a Folder")
    {
        if (_window.StorageProvider is null)
            return string.Empty;

        var folders = await _window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0
            ? Uri.UnescapeDataString(folders[0].Path.LocalPath)
            : string.Empty;
    }
}