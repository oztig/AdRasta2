using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Models;

namespace AdRasta2.Utils;

public class FileUtils
{
    public static bool CopyFile(string sourceFile, string destination)
    {
        if (sourceFile != null && destination != null)
        {
            var destinationDirectory = Path.GetDirectoryName(destination);
            return File.Exists(sourceFile) && Directory.Exists(destinationDirectory) &&
                   TryCopy(sourceFile, destination);
        }

        return false;
    }

    private static bool TryCopy(string source, string destination)
    {
        try
        {
            File.Copy(source, destination, true);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to Copy Files: " + e);
            return false;
        }
    }

    public static bool CreateFolder(string newFolderName)
    {
        try
        {
            if (!Directory.Exists(newFolderName))
                Directory.CreateDirectory(newFolderName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern,
        RastaConversion conversion, bool overwrite = true)
    {
        if (string.IsNullOrWhiteSpace(sourceDir))
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                "Source directory is null or empty. Aborting copy.");
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationDir))
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                "Destination directory is null or empty. Aborting copy.");
            return;
        }

        if (string.IsNullOrWhiteSpace(searchPattern))
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                "Search pattern is null or empty. Aborting copy.");
            return;
        }

        if (!Directory.Exists(sourceDir))
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                $"Source directory '{sourceDir}' does not exist. Aborting copy.");
            return;
        }

        if (!Directory.Exists(destinationDir))
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                $"Destination directory '{destinationDir}' does not exist. Aborting copy.");
            return;
        }

        var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);
        ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
            $"Found {files.Length} file(s) in '{sourceDir}' matching pattern '{searchPattern}'.");

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file))
                continue;

            var fileName = Path.GetFileName(file);
            var destPath = Path.Combine(destinationDir, fileName);

            if (!File.Exists(destPath) || overwrite)
            {
                File.Copy(file, destPath, overwrite);
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                    $"Copied '{fileName}' to '{destinationDir}' (overwrite: {overwrite}).");
            }
            else
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                    $"Skipped '{fileName}' — already exists and overwrite is false.");
            }
        }

        await Task.CompletedTask;
    }


    /*public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern,RastaConversion conversion, bool overwrite = true)
    {
        var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);
        ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug, $"Found {files.Length} file(s) in '{sourceDir}' matching pattern '{searchPattern}'.");

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var destPath = Path.Combine(destinationDir, fileName);

            if (!File.Exists(destPath) || overwrite)
            {
                File.Copy(file, destPath, overwrite);
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug, $"Copied '{fileName}' to '{destinationDir}' (overwrite: {overwrite}).");
            }
            else
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug, $"Skipped '{fileName}' — already exists and overwrite is false.");
            }
        }

        await Task.CompletedTask; // ceremonial async completion
    }*/


    // public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern,
    //     bool overwrite = true)
    // {
    //     var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);
    //
    //     foreach (var file in files)
    //     {
    //         var destPath = Path.Combine(destinationDir, Path.GetFileName(file));
    //
    //         if (!File.Exists(destPath))
    //             File.Copy(file, destPath, overwrite);
    //     }
    // }

    /// <summary>
    /// Copy file, converting any 'nasty' chars to underscores
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationDirectory"></param>
    /// <param name="sanitise"></param>
    /// <param name="overwrite"></param>
    /// <returns>If copied, and full sanitised filename (including directory)</returns>
    public static (bool success, string finalFileName) CopyFileWithSanitisation(
        RastaConversion conversion,
        string sourcePath,
        string destinationDirectory,
        bool sanitise = false,
        bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationDirectory))
            return (false, string.Empty);

        var originalFileName = Path.GetFileName(sourcePath);
        var finalFileName = sanitise ? SanitizeFileName(originalFileName) : originalFileName;
        var destPath = Path.Combine(destinationDirectory, finalFileName);

        try
        {
            if (!overwrite && File.Exists(destPath))
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                    $"Skipped copy: '{finalFileName}' already exists.");
                return (false, destPath);
            }

            File.Copy(sourcePath, destPath, overwrite);
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                $"Copied '{originalFileName}' as '{finalFileName}'");
            return (true, destPath);
        }
        catch (Exception ex)
        {
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error,
                $"Copy failed for '{finalFileName}': {ex.Message}");
            return (false, destPath);
        }
    }


    public static string SanitizeFileName(string fileName, bool stripSpaces = true)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var sanitized = stripSpaces
            ? fileName.Replace(" ", "_")
            : fileName;

        // Optional: remove other problematic characters
        sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_.-]", "_");

        return sanitized;
    }


    public static void DeleteMatchingFiles(string directory, string searchPattern)
    {
        var files = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                // Optional: log the failure, but don't interrupt the ritual
                // Debug.WriteLine($"Failed to delete {file}: {ex.Message}");
            }
        }
    }


    public static async Task CopyDirectoryIncludingRoot(string sourceDir, string destinationRoot)
    {
        string dirName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar));
        string destDir = Path.Combine(destinationRoot, dirName);
        await CopyDirectory(sourceDir, destDir);
    }

    public static async Task CopyDirectory(string sourceDir, string destDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            return;

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, recursive);
            }
        }
    }
}