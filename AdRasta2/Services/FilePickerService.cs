using System;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using AdRasta2.ViewModels.Helpers; // Assuming FileBrowseRequest lives here
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

    public async Task<string?> PickFileAsync(FileBrowseRequest request)
    {
        if (_window.StorageProvider is null)
            return null;

        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = $"Select file for {request.TargetProperty}",
            AllowMultiple = false,
            FileTypeFilter = new[] { request.FileType }
        });

        return files.Count > 0
            ? Uri.UnescapeDataString(files[0].Path.AbsolutePath)
            : null;
    }
}