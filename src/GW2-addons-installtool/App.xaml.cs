using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool;

public partial class App : Application
{
    private static readonly string LogPath = "log.txt";
    private readonly List<string> _installedSoftware = new List<string>();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += AppDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

        SetCulture();

        if (!CheckVcRedistributable())
        {
            MessageBox.Show("未检测到合适的 VC++ 2015-2022 Redistributable (x64) 运行库!\r\n为了确保游戏插件正常工作，请安装最新 VC++ 运行库。", "提醒");
        }
    }

    private bool CheckVcRedistributable()
    {
        // 1. Direct official Microsoft Visual Studio VC Runtime registry key check
        try
        {
            using RegistryKey? baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? vcKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
            if (vcKey != null)
            {
                int installed = Convert.ToInt32(vcKey.GetValue("Installed", 0));
                int major = Convert.ToInt32(vcKey.GetValue("Major", 0));
                int minor = Convert.ToInt32(vcKey.GetValue("Minor", 0));
                int bld = Convert.ToInt32(vcKey.GetValue("Bld", 0));

                if (installed == 1 && major >= 14 && (minor >= 14 || minor >= 27 || bld >= 27000))
                {
                    Global.vcv1 = major.ToString();
                    Global.vcv2 = minor.ToString();
                    Global.vcvs = $"{major}.{minor}.{bld}";
                    return true;
                }
            }
        }
        catch { }

        // 2. Fallback: Scan Uninstall Registry keys in both 64-bit and 32-bit views
        GetInstalledSoftwareList();

        if (_installedSoftware.Count > 0)
        {
            foreach (string item in _installedSoftware)
            {
                if (item.Contains("c++ 2015") || item.Contains("c++ 2017") || item.Contains("c++ 2019") || item.Contains("c++ 2022") || item.Contains("visual c++"))
                {
                    if (item.Contains("x64") || item.Contains("64-bit") || item.Contains("x64-"))
                    {
                        int dotIndex = item.IndexOf('.');
                        if (dotIndex >= 2 && dotIndex + 2 <= item.Length)
                        {
                            if (int.TryParse(item.Substring(dotIndex - 2, 2), out int v1) &&
                                int.TryParse(item.Substring(dotIndex + 1, 2), out int v2))
                            {
                                if (v1 >= 14)
                                {
                                    Global.vcv1 = v1.ToString();
                                    Global.vcv2 = v2.ToString();
                                    Global.vcvs = $"{v1}.{v2}";
                                    return true;
                                }
                            }
                        }

                        // If "2015-2022" or "2015-2019" is in display name for x64, treat as valid
                        if (item.Contains("2015-2022") || item.Contains("2015-2019") || item.Contains("2015-2017"))
                        {
                            Global.vcvs = "14.x";
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void SetCulture()
    {
        Global.GamePath = Iniflie.Read("general", "GamePath", "未设置", Iniflie.filename);
        if (int.TryParse(Iniflie.Read("general", "reshademode", "1", Iniflie.filename), out int rm))
        {
            Global.reshademode = rm;
        }
        if (int.TryParse(Iniflie.Read("general", "installpluginmode", "2", Iniflie.filename), out int pm))
        {
            Global.installpluginmode = pm;
        }
        if (int.TryParse(Iniflie.Read("general", "installdpsmode", "1", Iniflie.filename), out int dm))
        {
            Global.installdpsmode = dm;
        }
    }

    private void GetInstalledSoftwareList()
    {
        RegistryView[] views = new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 };
        RegistryHive[] hives = new RegistryHive[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser };

        foreach (RegistryHive hive in hives)
        {
            foreach (RegistryView view in views)
            {
                try
                {
                    using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using RegistryKey? uninstallKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                    if (uninstallKey != null)
                    {
                        foreach (string subkeyName in uninstallKey.GetSubKeyNames())
                        {
                            try
                            {
                                using RegistryKey? subKey = uninstallKey.OpenSubKey(subkeyName);
                                if (subKey?.GetValue("DisplayName") is string text && !string.IsNullOrEmpty(text))
                                {
                                    _installedSoftware.Add(text.ToLower());
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }
    }

    private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogError(LogPath, ex);
            if (ex is not WebException)
            {
                MessageBox.Show("发生了未处理的异常:\n" + ex.Message, "关键错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        LogError(LogPath, e.Exception);
        if (e.Exception is not WebException)
        {
            MessageBox.Show("发生了未处理的异常:\n" + e.Exception.Message, "关键错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LogError(string logfile, Exception ex)
    {
        try
        {
            string contents = $"[Log Entry {DateTime.Now}]\n{ex.Message}\n{ex.StackTrace}\n";
            File.AppendAllText(logfile, contents);
        }
        catch { }
    }
}
