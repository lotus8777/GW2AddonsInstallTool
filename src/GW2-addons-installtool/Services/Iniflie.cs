using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GW2_addons_installtool.Models;

namespace GW2_addons_installtool.Services;

public class Iniflie
{
    public static string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install.ini");

    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

    public static string Read(string section, string key, string def, string filePath)
    {
        if (!File.Exists(filePath))
        {
            return def;
        }
        StringBuilder stringBuilder = new StringBuilder(1024);
        GetPrivateProfileString(section, key, def, stringBuilder, 1024, filePath);
        return stringBuilder.ToString();
    }

    public static int Write(string section, string key, string value, string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }
        return WritePrivateProfileString(section, key, value, filePath);
    }

    public static void Save()
    {
        Write("general", "GamePath", Global.GamePath, filename);
        Write("general", "reshademode", Global.reshademode.ToString(), filename);
        Write("general", "installpluginmode", Global.installpluginmode.ToString(), filename);
        Write("general", "installdpsmode", Global.installdpsmode.ToString(), filename);
        foreach (Addon addon in Global.Addons)
        {
            Write("general", addon.Addon_name, addon.IsSelected.ToString(), filename);
        }
    }
}
