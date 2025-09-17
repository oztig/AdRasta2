using System;
using System.IO;
using System.Net;
using System.Text;

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
}