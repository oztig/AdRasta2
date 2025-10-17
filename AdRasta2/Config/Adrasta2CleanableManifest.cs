using System;
using System.Collections.Generic;
using System.Linq;
using AdRasta2.Models;

namespace AdRasta2.Config;

public static class AdRastaCleanableManifest
{
    public static readonly HashSet<string> RemovableFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "clacon2.ttf",
        "FreeImage.dll",
        "no_name.h",
        "no_name.asq",
        "RastaConverter.bmp",
        "SDL2.dll",
        "SDL2_ttf.dll"
    };

    public static readonly HashSet<string> RemovableDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Palettes"
    };

    public static readonly Dictionary<string, string[]> ScopedWildcardRules = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Palettes", ["*.act"] }  // Can also be specific filenames, without wild cards!
    };

    public static IEnumerable<string> GetEffectiveRemovableFileNames()
    {
        var names = RemovableFileNames.ToList();

        if (!string.IsNullOrEmpty(Settings.Current.BaseRastaCommand))
            names.Add(Settings.Current.BaseRastaCommand);
        
        if (!string.IsNullOrEmpty(Settings.Current.MadsLocationBaseName))
            names.Add(Settings.Current.MadsLocationBaseName);
        
        return names;
    }
}