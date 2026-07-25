using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            if (e.OriginalSource is DependencyObject obj)
            {
                if (obj is Button || FindParent<Button>(obj) != null)
                {
                    return;
                }
            }
            Window.GetWindow(this)?.DragMove();
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindParent<T>(parentObject);
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
        Task.Run(async () => await StartInstallProcessAsync());
    }

    private void AppendLog(string text)
    {
        Logger.Log(text.TrimEnd());
        Dispatcher.Invoke(() =>
        {
            textBox.Text += text;
            textBox.ScrollToEnd();
        });
    }

    public bool UnpackFiles(string dir, string file)
    {
        if (!File.Exists(file))
        {
            AppendLog($"[错误] 压缩文件不存在: {file}\r\n");
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
            AppendLog($"解压成功: {Path.GetFileName(file)} -> {dir}\r\n");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"解压失败: {file}", ex);
            MessageBox.Show(file + "解压失败:\r\n" + ex.Message + "\r\n您需要尝试重新点击安装", "解压失败");
            return false;
        }
    }

    private async Task StartInstallProcessAsync()
    {
        AppendLog($"===== 开始安装流程 [模式: {Global.installpluginmode}] =====\r\n");
        string cacheDir = Path.Combine(Global.GamePath, "Installcache");
        if (!Directory.Exists(cacheDir))
        {
            AppendLog("在游戏目录下创建存放目录Installcache\r\n");
            Directory.CreateDirectory(cacheDir);
        }

        int totalSelected = Global.Addons.Count(a => a.IsSelected);
        int currentAddonIndex = 0;

        foreach (Addon item in Global.Addons)
        {
            if (!item.IsSelected) continue;
            currentAddonIndex++;

            if (string.IsNullOrEmpty(item.Filename))
            {
                AppendLog($"[警告] 插件 [{item.Addon_name}] 暂未获取到服务器文件名，跳过该项\r\n");
                continue;
            }

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
                        AppendLog($"[{item.Addon_name}] {item.Filename} 文件MD5不一致 (本地:{fileMd5} vs 服务器:{item.Md5st}) 需重新下载\r\n");
                    }
                    else
                    {
                        AppendLog($"[{item.Addon_name}] {item.Filename} 文件MD5一致 无需下载\r\n");
                    }
                }
                else
                {
                    needDownload = true;
                    AppendLog($"[{item.Addon_name}] {item.Filename} 文件不存在 需下载\r\n");
                }

                if (needDownload)
                {
                    if (string.IsNullOrEmpty(item.Urlpath))
                    {
                        AppendLog($"[错误] 无法获取 [{item.Addon_name}] ({item.Filename}) 的下载链接，请检查网络设置\r\n");
                        continue;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        label.Content = $"({currentAddonIndex}/{totalSelected}) {item.Filename} 下载中...";
                        jindu.Value = 0;
                        jindu.Maximum = Math.Max(1, item.Filesize);
                    });

                    bool ok = await Downlodmeg.DownloadFileAsync(item.Urlpath, filePath, (read, total) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            jindu.Maximum = total > 0 ? total : Math.Max(1, item.Filesize);
                            jindu.Value = read;
                            double pct = jindu.Maximum > 0 ? (jindu.Value / jindu.Maximum * 100.0) : 0;
                            label.Content = $"({currentAddonIndex}/{totalSelected}) {item.Filename} 下载中... {pct:0.00}%";
                        });
                    });

                    if (!ok)
                    {
                        AppendLog($"[错误] [{item.Addon_name}] {item.Filename} 下载失败\r\n");
                    }
                }

                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (item.Filesize > 0 && fileInfo.Length < item.Filesize)
                    {
                        AppendLog($"[错误] [{item.Addon_name}] 压缩文件大小不一致 (本地:{fileInfo.Length} vs 服务器:{item.Filesize})\r\n");
                        MessageBox.Show($"{item.Addon_name} 压缩文件:{item.Filename}大小不一致,\r\n服务器文件大小:{item.Filesize}\r\n本地文件大小{fileInfo.Length}\r\n您需要点击返回重新开始安装!", "下载的文件大小不一致!");
                        Dispatcher.Invoke(() => back.IsEnabled = true);
                        return;
                    }
                    AppendLog($"[{item.Addon_name}] {item.Filename} 文件校验完成\r\n");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理插件 [{item.Addon_name}] 出错", ex);
                AppendLog($"[异常] {ex.Message}\r\n");
            }
        }

        Dispatcher.Invoke(() =>
        {
            label.Content = "清理老旧插件文件...";
            jindu.Value = 0;
            jindu.Maximum = 100;
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
                if (File.Exists(p1)) { File.Delete(p1); AppendLog($"清理 {fileItem}\r\n"); }
                string p2 = Path.Combine(bin64, fileItem);
                if (File.Exists(p2)) { File.Delete(p2); AppendLog($"清理 {fileItem}\r\n"); }
                string p3 = Path.Combine(bin64, "cef", fileItem);
                if (File.Exists(p3)) { File.Delete(p3); AppendLog($"清理 {fileItem}\r\n"); }
                string p4 = Path.Combine(arcdpsDir, fileItem);
                if (File.Exists(p4)) { File.Delete(p4); AppendLog($"清理 {fileItem}\r\n"); }
                string p5 = Path.Combine(addonsDir, fileItem);
                if (File.Exists(p5)) { File.Delete(p5); AppendLog($"清理 {fileItem}\r\n"); }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("清理旧文件出错", ex);
        }

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
        }

        AppendLog("===== 开始解压与安装所有已勾选插件 =====\r\n");
        foreach (Addon item in Global.Addons)
        {
            if (!item.IsSelected) continue;
            if (string.IsNullOrEmpty(item.Filename)) continue;

            string zipPath = Path.Combine(cacheDir, item.Filename);
            if (!File.Exists(zipPath))
            {
                AppendLog($"[警告] 文件未找到，跳过解压: {item.Filename}\r\n");
                continue;
            }

            string targetUnpackDir = gameRoot;

            if (item.Id == 0 || item.Id == 10 || item.Id == 14) // ARCDPS(0), ReShade(10), Nexus核心(14) 包含核心加载器, 必须部署于游戏根目录
            {
                targetUnpackDir = gameRoot;
            }
            else if (Global.installpluginmode == 2) // Nexus 模式下的其他扩展子插件解压至 addons 目录
            {
                targetUnpackDir = addonsDir;
            }
            else // 正常模式 (0) 或 疑难模式 (1) 全部部署于游戏根目录
            {
                targetUnpackDir = gameRoot;
            }

            AppendLog($"正在解压 [{item.Addon_name}] ({item.Filename}) 到 {targetUnpackDir}...\r\n");
            if (UnpackFiles(targetUnpackDir + "\\", zipPath))
            {
                if (Global.installpluginmode == 2 && item.Id == 0)
                {
                    if (File.Exists(Path.Combine(addonsDir, "d3d9.dll")))
                    {
                        File.Move(Path.Combine(addonsDir, "d3d9.dll"), Path.Combine(addonsDir, "ArcDPS.dll"), overwrite: true);
                    }
                }
                AppendLog($"[{item.Addon_name}] 解压并安装成功！\r\n");
            }
        }

        // 关键保障：校验并自动部署 DirectX 11 核心挂载文件 (d3d11.dll) 到游戏根目录 (Gw2-64.exe 同级)
        string rootD3d11 = Path.Combine(gameRoot, "d3d11.dll");
        string addonsD3d11 = Path.Combine(addonsDir, "d3d11.dll");
        if (!File.Exists(rootD3d11) && File.Exists(addonsD3d11))
        {
            File.Copy(addonsD3d11, rootD3d11, overwrite: true);
            AppendLog("已自动将核心加载器 d3d11.dll 部署至游戏根目录\r\n");
        }

        if (Directory.Exists(peiziDir))
        {
            try { Directory.Delete(peiziDir, recursive: true); } catch { }
        }

        Dispatcher.Invoke(() =>
        {
            AppendLog("===== 所有插件安装全部完成 =====\r\n");
            label.Content = "安装完成";
            jindu.Value = jindu.Maximum;
            back.IsEnabled = true;
        });

        Logger.Log("安装流程全部完成。");
        MessageBox.Show("安装完成\r\n请正常启动游戏查看是否生效", "安装完成!");
    }
}
