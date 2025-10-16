using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;

namespace AdRasta2.Models;

public class ConversionCleaner
{
    private static readonly string[] PreservedExtensions =
        { ".png", ".jpg", ".bmp", ".json", ".cfg", ".ini", ".csv", ".xlsx", ".txt", ".webp",".mch",".xex" };

    private static readonly List<string> PreservedPrefixes = new()
    {
        "output",
        "out_"
        // Add more as needed
    };

    public static async Task<CleanupResult> CleanupConversionAsync(RastaConversion conversion, bool dryRun = false)
    {
        var removedFiles = new List<string>();
        var preservedFiles = new List<string>();
        var removedDirectories = new List<string>();

        await Task.Run(() =>
        {
            // Sweep files
            foreach (var file in Directory.GetFiles(conversion.DestinationDirectory, "*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                string name = Path.GetFileName(file).ToLowerInvariant();

                bool isPreservedByExtension = PreservedExtensions.Contains(ext);
                bool isPreservedByPrefix = PreservedPrefixes.Any(prefix => name.StartsWith(prefix));

                if (isPreservedByExtension || isPreservedByPrefix)
                {
                    preservedFiles.Add(file);
                }
                else
                {
                    removedFiles.Add(file);
                    ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupInProgress,
                        dryRun ? $"[DryRun] Would remove: {name}" : $"Removed: {name}");

                    if (!dryRun)
                        File.Delete(file);
                }
            }

            // Simulate post-cleanup state for directory sweep
            var filesToRemove = new HashSet<string>(removedFiles, StringComparer.OrdinalIgnoreCase);

            var allDirectories = Directory
                .GetDirectories(conversion.DestinationDirectory, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length);

            foreach (var dir in allDirectories)
            {
                var entries = Directory.GetFileSystemEntries(dir);

                bool wouldBeEmpty = entries.All(entry =>
                {
                    if (Directory.Exists(entry))
                        return false; // Subdirectory exists

                    return filesToRemove.Contains(entry); // File would be removed
                });

                if (wouldBeEmpty)
                {
                    removedDirectories.Add(dir);
                    var dirName = Path.GetRelativePath(conversion.DestinationDirectory, dir);
                    ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupInProgress,
                        dryRun
                            ? $"[DryRun] Would remove empty directory: {dirName}"
                            : $"Removed empty directory: {dirName}");

                    if (!dryRun)
                        Directory.Delete(dir);
                }
            }
        });

        ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupComplete,
            dryRun
                ? $"[DryRun] Cleanup preview: {removedFiles.Count} files and {removedDirectories.Count} directories would be removed, {preservedFiles.Count} files retained."
                : $"Cleanup complete: {removedFiles.Count} files and {removedDirectories.Count} directories removed, {preservedFiles.Count} files retained.");

        return new CleanupResult(removedFiles, preservedFiles, removedDirectories);
    }
}

public record CleanupResult(
    List<string> RemovedFiles,
    List<string> PreservedFiles,
    List<string> RemovedDirectories);