using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

    public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern,bool overwrite = true)
    {
        var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var destPath = Path.Combine(destinationDir, Path.GetFileName(file));
            
            if (!File.Exists(destPath))
                File.Copy(file, destPath, overwrite);
        }
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