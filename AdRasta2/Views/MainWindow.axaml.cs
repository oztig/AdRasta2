using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

namespace AdRasta2.Views;

public partial class MainWindow : Window
{
    private double _initialMaxHeight;
    private double _initialMaxWidth;
    private bool _userResized = false;
    
    public MainWindow()
    {
        InitializeComponent();

        Settings.Current.LoadFromIni(Settings.ApplicationDebugLog);

        this.Opened += (_, _) =>
        {
            var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary;
            var scale = screen.Scaling;
            _initialMaxHeight = (screen.WorkingArea.Height / scale) * 0.75;
            _initialMaxWidth = (screen.WorkingArea.Width / scale) * 0.45;

            this.Height = _initialMaxHeight;
            this.Width = _initialMaxWidth;
            this.SizeToContent = SizeToContent.Manual;
        };

        this.GetObservable(ClientSizeProperty).Subscribe(OnClientSizeChanged);
    }


    
    private void OnClientSizeChanged(Size newSize)
    {
        if (_userResized) return;
        
        if (newSize.Height > _initialMaxHeight)
            this.MaxHeight = double.PositiveInfinity; // Lift the ceiling
        
        if (newSize.Width > _initialMaxWidth)
            this.MaxWidth = double.PositiveInfinity; // Lift the ceiling
    }
    

    private async Task<bool> CheckIniFileExists()
    {
        if (!Settings.CheckIniFileExists())
        {
            // Show a warning and exit
            var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = "Cannot find Adrasta2.ini file",
                ContentMessage = "Unable to find :" + Settings.IniFileLocation,
                ButtonDefinitions = new List<ButtonDefinition>
                {
                    new ButtonDefinition { Name = "Okay" },
                },
                ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            var result = await messageBox.ShowWindowDialogAsync(this);
            Environment.Exit(-1);
            return false;
        }

        return true;
    }
}