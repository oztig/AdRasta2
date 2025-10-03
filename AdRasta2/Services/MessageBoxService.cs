using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace AdRasta2.Services;

public class MessageBoxService : IMessageBoxService
{
    public async Task<string?> ShowConfirmationAsync(string title, string message,Icon icon = Icon.None)
    {
        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = title,
            ContentMessage = message,
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "Okay" },
                new ButtonDefinition { Name = "Cancel" }
            },
            Icon = icon,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        return await messageBox.ShowAsync();
    }
    
    public Task ShowInfoAsync(string title, string message,double maxHeight=500)
    {
       var iconPath = new Uri($"avares://AdRasta2/Assets/info.png");
       var bitmap = new Bitmap(AssetLoader.Open(iconPath));
        
        var msgBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = title,
            ContentMessage = message,
            ButtonDefinitions = new List<ButtonDefinition> { new ButtonDefinition { Name = "OK" } },
            Icon = Icon.None,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ImageIcon = bitmap,
            MaxHeight = maxHeight
        });

        return msgBox.ShowAsync();
    }

}
