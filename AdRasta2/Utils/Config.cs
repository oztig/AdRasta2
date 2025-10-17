using System;
using System.Collections.Generic;
using System.IO;
using Sini;

public class Config
{
    private readonly IniFile _ini;
    private readonly string _sourcePath;
    private readonly Dictionary<string, Dictionary<string, string>> _data;

    // public Config(string iniPath)
    // {
    //     _sourcePath = iniPath;
    //     var iniText = File.ReadAllText(iniPath);
    //     _ini = new IniFile(iniText);
    //     _data = new Dictionary<string, Dictionary<string, string>>();
    //
    //     foreach (var section in _ini.Sections)
    //     {
    //         var sectionData = new Dictionary<string, string>();
    //         foreach (var kvp in section.Value)
    //         {
    //             sectionData[kvp.Key] = kvp.Value;
    //         }
    //         _data[section.Key] = sectionData;
    //     }
    // }

    public string GetSafeString(string section, string key, string fallback)
    {
        if (_data.TryGetValue(section, out var sectionData) &&
            sectionData.TryGetValue(key, out var value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        Log($"[Config][{section}:{key}] Missing or empty, using fallback: '{fallback}'");
        return fallback;
    }

    public bool GetSafeBool(string section, string key, bool fallback)
    {
        var str = GetSafeString(section, key, fallback.ToString());
        return bool.TryParse(str, out var result) ? result : fallback;
    }

    public double GetSafeDouble(string section, string key, double fallback)
    {
        var str = GetSafeString(section, key, fallback.ToString());
        return double.TryParse(str, out var result) ? result : fallback;
    }

    public int GetSafeInt(string section, string key, int fallback)
    {
        var str = GetSafeString(section, key, fallback.ToString());
        return int.TryParse(str, out var result) ? result : fallback;
    }

    public void Set(string section, string key, string value)
    {
        if (!_data.ContainsKey(section))
            _data[section] = new Dictionary<string, string>();

        _data[section][key] = value;
        Log($"[Config][{section}:{key}] Set to '{value}'");
    }

    // public void Save(string? pathOverride = null)
    // {
    //     var path = pathOverride ?? _sourcePath;
    //     var lines = new List<string>();
    //
    //     foreach (var section in _data)
    //     {
    //         lines.Add($"[{section.Key}]");
    //         foreach (var kvp in section.Value)
    //             lines.Add($"{kvp.Key}={kvp.Value}");
    //         lines.Add(""); // spacing
    //     }
    //
    //     File.WriteAllLines(path, lines);
    //     Log($"[Config] Saved to '{path}'");
    // }

    private void Log(string message)
    {
        Console.WriteLine(message); // Replace with your logging system if needed
    }
}
