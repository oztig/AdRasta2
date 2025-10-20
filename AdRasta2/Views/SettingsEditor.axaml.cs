using AdRasta2.Services;
using AdRasta2.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;

namespace AdRasta2.Views;

public partial class SettingsEditor : Window
{
    
    public SettingsEditor()
    {
        InitializeComponent();
        var filePickerService = new FilePickerService(this);
        var folderPickerService = new FolderPickerService(this);
        var viewModel = new SettingsEditorViewModel(filePickerService, folderPickerService);
        
        viewModel.CloseEditorAction = Close;
        
        DataContext = viewModel;
        UnstuckAfter.DefaultIfNull = viewModel.DefaultUnstuckAfter;
        UnstuckDrift.DefaultIfNull = viewModel.DefaultUnstuckDrift;
    }
}