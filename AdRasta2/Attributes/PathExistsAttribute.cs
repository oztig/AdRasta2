using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AdRasta2.Attributes;

public class PathExistsAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var path = value as string;
        return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
    }
}
