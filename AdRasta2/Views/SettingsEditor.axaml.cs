using System;
using AdRasta2.Services;
using AdRasta2.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.Linq;
using AdRasta2.Models;
using Avalonia.Threading;

namespace AdRasta2.Views;

public enum SettingsEditorResult
{
    Saved,
    Cancelled
}

public partial class SettingsEditor : Window
{
    
    public SettingsEditor(SourceData dataSource)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var filePickerService = new FilePickerService(this);
        var folderPickerService = new FolderPickerService(this);
        var viewModel = new SettingsEditorViewModel(filePickerService, folderPickerService,dataSource);
        
        viewModel.CloseEditorAction = result => Close(result);
        
        DataContext = viewModel;
        UnstuckAfter.DefaultIfNull = viewModel.DefaultUnstuckAfter;
        UnstuckDrift.DefaultIfNull = viewModel.DefaultUnstuckDrift;
    }
    
}