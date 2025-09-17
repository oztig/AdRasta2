using System.Threading.Tasks;

namespace AdRasta2.Interfaces;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync(string title = "Select a folder");
}