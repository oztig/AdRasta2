using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdRasta2.Config;
using AdRasta2.Enums;
using AdRasta2.Utils;

namespace AdRasta2.Models;

public class ConversionCleaner
{
    public static async Task<CleanupResult> CleanupConversionAsync(RastaConversion conversion, bool dryRunDelete = false)
    {
        var removedFiles = new List<string>();
        var preservedFiles = new List<string>();
        var removedDirectories = new List<string>();

        await Task.Run(() =>
        {
            // Sweep files
            foreach (var file in Directory.GetFiles(conversion.DestinationDirectory, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                string fileDir = Path.GetDirectoryName(file) ?? "";
                string relativeDir = Path.GetRelativePath(conversion.DestinationDirectory, fileDir);

                bool matchesScopedWildcard =
                    AdRastaCleanableManifest.ScopedWildcardRules.TryGetValue(relativeDir, out var patterns)
                    && patterns.Any(pattern => MatchesWildcard(fileName, pattern));

                var effectiveRemovableNames = AdRastaCleanableManifest.GetEffectiveRemovableFileNames();

                if (effectiveRemovableNames.Contains(fileName) || matchesScopedWildcard)
                {
                    removedFiles.Add(file);
                    ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupInProgress,
                        dryRunDelete ? $"[DryRunDelete] Would remove: {fileName}" : $"Removed: {fileName}");

                    if (!dryRunDelete)
                        File.Delete(file);
                }
                else
                {
                    preservedFiles.Add(file);
                }
            }

            // Simulate post-cleanup state for directory sweep
            var filesToRemove = new HashSet<string>(removedFiles, StringComparer.OrdinalIgnoreCase);

            var allDirectories = Directory
                .GetDirectories(conversion.DestinationDirectory, "*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.Length);

            foreach (var dir in allDirectories)
            {
                string dirNameOnly = Path.GetFileName(dir);

                if (!AdRastaCleanableManifest.RemovableDirectoryNames.Contains(dirNameOnly))
                    continue;

                var entries = Directory.GetFileSystemEntries(dir);

                bool wouldBeEmpty = dryRunDelete
                    ? entries.All(entry => filesToRemove.Contains(entry))
                    : entries.Length == 0;

                if (wouldBeEmpty)
                {
                    removedDirectories.Add(dir);
                    var relativeDir = Path.GetRelativePath(conversion.DestinationDirectory, dir);
                    ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupInProgress,
                        dryRunDelete
                            ? $"[DryRunDelete] Would remove empty directory: {relativeDir}"
                            : $"Removed empty directory: {relativeDir}");

                    if (!dryRunDelete)
                        Directory.Delete(dir);
                }
            }
        });

        ConversionLogger.LogIfDebug(conversion, ConversionStatus.CleanupComplete,
            dryRunDelete
                ? $"[DryRunDelete] Cleanup preview: {removedFiles.Count} files and {removedDirectories.Count} directories would be removed, {preservedFiles.Count} files retained."
                : $"Cleanup complete: {removedFiles.Count} files and {removedDirectories.Count} directories removed, {preservedFiles.Count} files retained.");

        return new CleanupResult(removedFiles, preservedFiles, removedDirectories);
    }

    private static bool MatchesWildcard(string fileName, string pattern)
    {
        return Regex.IsMatch(
            fileName,
            "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$",
            RegexOptions.IgnoreCase);
    }
}

public record CleanupResult(
    List<string> RemovedFiles,
    List<string> PreservedFiles,
    List<string> RemovedDirectories);