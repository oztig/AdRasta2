using System.Threading.Tasks;

namespace AdRasta2.Interfaces;

public interface IMessageBoxService
{
    Task<string?> ShowConfirmationAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
}
