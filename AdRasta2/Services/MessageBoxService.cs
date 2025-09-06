using System.Collections.Generic;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace AdRasta2.Services;

public class MessageBoxService : IMessageBoxService
{
    public async Task<string?> ShowConfirmationAsync(string title, string message)
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
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        return await messageBox.ShowAsync();
    }
    
    public async Task ShowInfoAsync(string title, string message)
    {
        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            ButtonEnum.Ok,
            Icon.Info
        );

        await msgBox.ShowAsync();
    }
}
