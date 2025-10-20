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

    public async Task<string> PickFolderAsync(string title = "Select a Folder", string? initialFolder = null)
    {
        if (_window.StorageProvider is null)
            return string.Empty;

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        if (!string.IsNullOrWhiteSpace(initialFolder))
        {
            var folder = await _window.StorageProvider.TryGetFolderFromPathAsync(initialFolder);
            if (folder is not null)
                options.SuggestedStartLocation = folder;
        }

        var folders = await _window.StorageProvider.OpenFolderPickerAsync(options);

        return folders.Count > 0
            ? Uri.UnescapeDataString(folders[0].Path.LocalPath)
            : string.Empty;
    }
}