using System.ComponentModel.DataAnnotations;
using System.IO;
using AdRasta2.Models;

namespace AdRasta2.Attributes;

public class FileExistsIfWindowsAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (!Settings.IsWindows) return true;
        
        var path = value as string;
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }
}