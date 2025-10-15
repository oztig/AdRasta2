using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using AdRasta2.Enums;
using AdRasta2.Models;
using Avalonia.Controls;

namespace AdRasta2.ViewModels;

public class LogViewerViewModel : ViewModelBase
{
    private readonly Window _window;
    public ObservableCollection<StatusEntry> LogEntries { get; } = new();
    public string Title { get; set; } = "Log";

    public LogViewerViewModel(IEnumerable<StatusEntry> entries, string title,Window window)
    {
        _window = window;
        
        foreach (var entry in entries)
            LogEntries.Add(entry);

        Title = title;
    }
    
    public async void CopyLogToClipboardAsync()
    {
        var logText = string.Join(Environment.NewLine,
            LogEntries.Select(s => s.Message));
        
        if (!string.IsNullOrWhiteSpace(logText))
            await _window.Clipboard.SetTextAsync(logText);
    }
}

