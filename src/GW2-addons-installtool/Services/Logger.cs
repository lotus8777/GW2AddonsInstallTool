using System;
using System.IO;
using System.Text;

namespace GW2_addons_installtool.Services;

public static class Logger
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "run_log.txt");
    private static readonly object LockObj = new object();

    public static string LogPath => LogFilePath;

    public static void Log(string message)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            lock (LockObj)
            {
                File.AppendAllText(LogFilePath, line, Encoding.UTF8);
            }
        }
        catch { }
    }

    public static void LogError(string context, Exception ex)
    {
        Log($"[ERROR] {context}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
    }
}
