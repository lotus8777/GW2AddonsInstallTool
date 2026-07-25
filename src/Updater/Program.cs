using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace UOAOM_Updater;

internal class Program
{
    private static readonly string UpdateZip = "latestRelease\\update.zip";
    private static readonly string UpdateFolder = "latestRelease";

    private static void Main(string[] args)
    {
        Thread.Sleep(3000);
        if (!Directory.Exists(UpdateFolder) || !File.Exists(UpdateZip))
        {
            return;
        }

        try
        {
            using (FileStream fileStream = new FileStream(UpdateZip, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(entry.FullName);
                    }
                    else if (!entry.FullName.Equals("UOAOM updater.exe", StringComparison.OrdinalIgnoreCase) &&
                             !entry.FullName.Equals("Updater.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string destPath = Path.Combine(Directory.GetCurrentDirectory(), entry.FullName);
                        string? dirName = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }

                        try
                        {
                            entry.ExtractToFile(destPath, overwrite: true);
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(3000);
                            entry.ExtractToFile(destPath, overwrite: true);
                        }
                    }
                }
            }

            Directory.Delete(UpdateFolder, recursive: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed: {ex.Message}");
        }
    }
}
