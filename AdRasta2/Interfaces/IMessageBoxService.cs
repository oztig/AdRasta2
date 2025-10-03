using System.Threading.Tasks;
using MsBox.Avalonia.Enums;

namespace AdRasta2.Interfaces;

public interface IMessageBoxService
{
    Task<string?> ShowConfirmationAsync(string title, string message,Icon icon = Icon.None);
    Task ShowInfoAsync(string title, string message,double maxHeight=500);
}
