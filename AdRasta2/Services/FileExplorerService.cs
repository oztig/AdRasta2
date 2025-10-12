using System;
using System.Diagnostics;
using System.IO;
using AdRasta2.Interfaces;
using AdRasta2.Models;

namespace AdRasta2.Services;

public class FileExplorerService : IFileExplorerService
{
    private readonly string _defaultCommand;

    public FileExplorerService()
    {
        _defaultCommand = Settings.IsWindows ? "explorer"
            : OperatingSystem.IsLinux() ? "xdg-open"
            : "explorer"; // fallback
    }

    public void OpenFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _defaultCommand,
                Arguments = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Log or annotate the failure with timestamp and path
        }
    }
}