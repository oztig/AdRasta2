using System.Collections.Generic;
using AdRasta2.Models;
using AdRasta2.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ApplicationLogView : Window
{
    public ApplicationLogView(IEnumerable<StatusEntry> statusEntries,Window window)
    {
        InitializeComponent();
        DataContext = new LogViewerViewModel(statusEntries, "Application Log",window);
    }
}