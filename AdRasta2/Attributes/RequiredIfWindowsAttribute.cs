using System.ComponentModel.DataAnnotations;
using AdRasta2.Models;

namespace AdRasta2.Attributes;

public class RequiredIfWindowsAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (!Settings.IsWindows) return true;

        var str = value as string;
        return !string.IsNullOrWhiteSpace(str);
    }
}
