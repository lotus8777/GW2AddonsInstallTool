using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GW2_addons_installtool.Models;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.UI;

public partial class Install : Page
{
    public double 项目数;
    public double 进度数据 { get; set; }
    public string 正在进行的项目 { get; set; } = string.Empty;

    public Install()
    {
        InitializeComponent();
        back.IsEnabled = false;
    }

    private void TitleBar_MouseHeld(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            Window.GetWindow(this)?.DragMove();
        }
    }

    private void close_clicked(object sender, RoutedEventArgs e)
    {
        Iniflie.Save();
        Application.Current.Shutdown();
    }

    private void minimize_clicked(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window win)
        {
            win.WindowState = WindowState.Minimized;
        }
    }

    private void back_clicked(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new Uri("UI/Settings.xaml", UriKind.Relative));
    }

    private void ispageloaded(object sender, RoutedEventArgs e)
    {
        项目数 = 0;
        foreach (Addon addon in Global.Addons)
        {
            if (addon.IsSelected)
            {
                项目数 += 1.0;
            }
        }
        jindu.Maximum = 100.0;
        Task.Run(() => download());
    }

    public static bool UnpackFiles(string dir, string file)
    {
        if (!File.Exists(file))
        {
            MessageBox.Show(file + "不存在\r\n您需要尝试重新点击安装", "解压失败");
            return false;
        }
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        string subDir = Path.Combine(dir, entry.FullName);
                        if (!Directory.Exists(subDir)) Directory.CreateDirectory(subDir);
                        continue;
                    }

                    if ((entry.Name.Equals("DefaultPreset.ini", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dir, entry.Name))) ||
                        (entry.Name.Equals("ReShade.ini", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dir, entry.Name))) ||
                        (entry.Name.Equals("GShade.ini", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dir, entry.Name))) ||
                        (entry.Name.Equals("Off.ini", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(dir, entry.Name))))
                    {
                        continue;
                    }

                    string destFile = Path.Combine(dir, entry.FullName);
                    string? destDir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    entry.ExtractToFile(destFile, overwrite: true);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(file + "解压失败:\r\n" + ex.Message + "\r\n您需要尝试重新点击安装", "解压失败");
            return false;
        }
    }

    private void download()
    {
        string cacheDir = Path.Combine(Global.GamePath, "Installcache");
        if (!Directory.Exists(cacheDir))
        {
            Dispatcher.Invoke(() =>
            {
                textBox.Text += "在游戏目录下创建存放目录Installcache\r\n";
            });
            Directory.CreateDirectory(cacheDir);
        }

        foreach (Addon item in Global.Addons)
        {
            if (!item.IsSelected) continue;

            try
            {
                bool needDownload = false;
                string filePath = Path.Combine(cacheDir, item.Filename);

                if (File.Exists(filePath))
                {
                    using FileStream inputStream = File.OpenRead(filePath);
                    using MD5 md5 = MD5.Create();
                    string fileMd5 = md5.ComputeHash(inputStream).Aggregate(string.Empty, (res, b) => res + b.ToString("X2")).ToLower();

                    if (!fileMd5.Equals(item.Md5st, StringComparison.OrdinalIgnoreCase))
                    {
                        needDownload = true;
                        Dispatcher.Invoke(() =>
                        {
                            textBox.Text += item.Filename + "文件MD5不一致 需下载\r\n";
                            textBox.ScrollToEnd();
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            textBox.Text += item.Filename + "文件MD5一致 无需下载\r\n";
                            textBox.ScrollToEnd();
                        });
                    }
                }
                else
                {
                    needDownload = true;
                    Dispatcher.Invoke(() =>
                    {
                        textBox.Text += item.Filename + "文件不存在 需下载\r\n";
                        textBox.ScrollToEnd();
                    });
                }

                if (needDownload && !string.IsNullOrEmpty(item.Urlpath))
                {
                    Dispatcher.Invoke(() =>
                    {
                        label.Content = item.Filename + "下载中...";
                    });
                    Downlodmeg downloader = new Downlodmeg(item.Urlpath, item.Filename, 1);
                    Task.Delay(100).Wait();

                    while (!downloader.taskA.IsCompleted)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            jindu.Maximum = Math.Max(1, item.Filesize);
                            jindu.Value = downloader.Progresss;
                            label.Content = item.Filename + "下载中..." + ((double)jindu.Value / jindu.Maximum * 100.0).ToString("0.00") + "%";
                        });
                        Task.Delay(50).Wait();
                    }
                    Task.Delay(500).Wait();
                }

                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (item.Filesize > 0 && fileInfo.Length < item.Filesize)
                    {
                        MessageBox.Show($"{item.Addon_name} 压缩文件:{item.Filename}大小不一致,\r\n服务器文件大小:{item.Filesize}\r\n本地文件大小{fileInfo.Length}\r\n您需要点击返回重新开始安装!", "下载的文件大小不一致!");
                        Dispatcher.Invoke(() =>
                        {
                            back.IsEnabled = true;
                        });
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        textBox.Text += item.Filename + "文件大小一致\r\n";
                        textBox.ScrollToEnd();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        string dpsTitle = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        string arcdpsLangFile = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps_lang.ini");
        if (File.Exists(arcdpsLangFile))
        {
            dpsTitle = Iniflie.Read("lang", "703", "", arcdpsLangFile);
            Dispatcher.Invoke(() =>
            {
                textBox.Text += "已读取arcdps_lang.ini文件中的DPS标题栏\r\n";
                textBox.ScrollToEnd();
            });
        }

        Dispatcher.Invoke(() =>
        {
            textBox.Text += "开始清理文件\r\n";
            label.Content = "清理文件...";
            textBox.ScrollToEnd();

            if (Global.installpluginmode == 0)
            {
                jindu.Maximum = (项目数 + 1.0) * 10.0;
            }
            else if (Global.installpluginmode == 1)
            {
                jindu.Maximum = (项目数 - 1.0) * 10.0;
            }
            else if (Global.installpluginmode == 2)
            {
                jindu.Maximum = 项目数 * 10.0;
            }
            jindu.Value = 0.0;
        });

        string bin64 = Path.Combine(Global.GamePath, "bin64");
        string gameRoot = Global.GamePath;
        string arcdpsDir = Path.Combine(Global.GamePath, @"addons\arcdps");
        string addonsDir = Path.Combine(Global.GamePath, "addons");

        string[] cleanFiles = new string[]
        {
            "d3d9.dll", "d3d9.dll.md5sum", "d3d9_arcdps_buildtemplates.dll", "d3d9_arcdps_extras.dll", "d3d9_arcdps_mechanicschs.dll", "d3d9_arcdps_mechanics.dll", "d3d9_arcdps_buildpad.dll", "d3d9_arcdps_sytool.dll", "d3d9_arcdps_ct.dll", "d3d9_arcdps_noex.dll",
            "d3d9_chainload.dll", "d3d9_chainload_noex.dll", "d3d9_arcdps_tablechs.dll", "d3d9_arcdps_table.dll", "d3d9_arcdps_sct.dll", "arcdps_healing_stats.dll", "ReShade64.dll", "ReShade.ini", "DefaultPreset.ini", "d3d9_mchain.dll",
            "SweetFX readme.txt", "SweetFX_preset.txt", "SweetFX_settings.txt", "dxgi.dll", "d3d9_ReShade641.zip", "d3d9_arcdps_MountTool.dll", "d3d9_arcdps_SY_CN_Tool.dll", "d912pxy.dll", "SweetFX.zip", "ReShade.fx",
            "Sweet.fx", "dxgi.log", "d3d9_mchain.log", "ReShade64.log", "log.log", "gw2addon_arcdps.dll", "addonLoader.dll", "d3d11.dll", "gw2addon_ReShade64.dll", "GShade.ini",
            "d3d9_arcdps_SY_zc.dll", "gw2clarity.log", "gw2radial.log", "gw2sycntool.log", "ReShade.log", "gw2al_log.txt", "ArcDPS.dll", "MountTool.dll", "SY_Tool.dll", "SY_zc.dll",
            "Boon_Table.dll", "Healing_Stats.dll"
        };

        try
        {
            foreach (string fileItem in cleanFiles)
            {
                string p1 = Path.Combine(gameRoot, fileItem);
                if (File.Exists(p1))
                {
                    File.Delete(p1);
                    Dispatcher.Invoke(() => { textBox.Text += "删除" + p1 + "\r\n"; textBox.ScrollToEnd(); });
                }
                string p2 = Path.Combine(bin64, fileItem);
                if (File.Exists(p2))
                {
                    File.Delete(p2);
                    Dispatcher.Invoke(() => { textBox.Text += "删除" + p2 + "\r\n"; textBox.ScrollToEnd(); });
                }
                string p3 = Path.Combine(bin64, "cef", fileItem);
                if (File.Exists(p3))
                {
                    File.Delete(p3);
                    Dispatcher.Invoke(() => { textBox.Text += "删除" + p3 + "\r\n"; textBox.ScrollToEnd(); });
                }
                string p4 = Path.Combine(arcdpsDir, fileItem);
                if (File.Exists(p4))
                {
                    File.Delete(p4);
                    Dispatcher.Invoke(() => { textBox.Text += "删除" + p4 + "\r\n"; textBox.ScrollToEnd(); });
                }
                string p5 = Path.Combine(addonsDir, fileItem);
                if (File.Exists(p5))
                {
                    File.Delete(p5);
                    Dispatcher.Invoke(() => { textBox.Text += "删除" + p5 + "\r\n"; textBox.ScrollToEnd(); });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + "\r\n\r\n清理文件失败,请尝试重启电脑后再安装插件!!!", "提醒!");
        }

        Dispatcher.Invoke(() =>
        {
            textBox.Text += "清理完成\r\n";
            label.Content = "开始解压并安装文件...";
            textBox.ScrollToEnd();
            jindu.Value = 10.0;
        });

        if (!Directory.Exists(arcdpsDir)) Directory.CreateDirectory(arcdpsDir);
        if (!Directory.Exists(bin64)) Directory.CreateDirectory(bin64);
        if (!Directory.Exists(Path.Combine(bin64, "cef"))) Directory.CreateDirectory(Path.Combine(bin64, "cef"));
        if (!Directory.Exists(Path.Combine(addonsDir, @"sct\fonts"))) Directory.CreateDirectory(Path.Combine(addonsDir, @"sct\fonts"));

        string peiziDir = Path.Combine(addonsDir, "peizi");
        Directory.CreateDirectory(peiziDir);
        string peiziZip = Path.Combine(cacheDir, "peizi.zip");
        if (File.Exists(peiziZip))
        {
            UnpackFiles(peiziDir + "\\", peiziZip);
        }

        if (Global.installpluginmode == 2)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(addonsDir, "Nexus"))) Directory.CreateDirectory(Path.Combine(addonsDir, "Nexus"));
                if (!Directory.Exists(Path.Combine(addonsDir, @"Nexus\Fonts"))) Directory.CreateDirectory(Path.Combine(addonsDir, @"Nexus\Fonts"));

                if (File.Exists(Path.Combine(peiziDir, "Settings.json")) && !File.Exists(Path.Combine(addonsDir, @"Nexus\Settings.json")))
                {
                    File.Copy(Path.Combine(peiziDir, "Settings.json"), Path.Combine(addonsDir, @"Nexus\Settings.json"), overwrite: true);
                }
                if (File.Exists(Path.Combine(peiziDir, "arcdps_font.ttf")))
                {
                    File.Copy(Path.Combine(peiziDir, "arcdps_font.ttf"), Path.Combine(addonsDir, @"Nexus\Fonts\arcdps_font.ttf"), overwrite: true);
                }

                if (Global.Addons[0].IsSelected)
                {
                    string arcZip = Path.Combine(cacheDir, Global.Addons[0].Filename);
                    if (UnpackFiles(addonsDir + "\\", arcZip))
                    {
                        Dispatcher.Invoke(() => { textBox.Text += Global.Addons[0].Filename + "文件解压完成\r\n"; textBox.ScrollToEnd(); });
                    }
                    if (File.Exists(Path.Combine(addonsDir, "d3d9.dll")))
                    {
                        File.Move(Path.Combine(addonsDir, "d3d9.dll"), Path.Combine(addonsDir, "ArcDPS.dll"), overwrite: true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        if (Directory.Exists(peiziDir))
        {
            try { Directory.Delete(peiziDir, recursive: true); } catch { }
        }

        Dispatcher.Invoke(() =>
        {
            textBox.Text += "删除临时目录\r\n安装完成\r\n";
            label.Content = "安装完成";
            textBox.ScrollToEnd();
            jindu.Value = jindu.Maximum;
            back.IsEnabled = true;
        });

        MessageBox.Show("安装完成\r\n请正常启动游戏查看是否生效", "安装完成!");
    }
}
