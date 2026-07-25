using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Win32;
using GW2_addons_installtool.Models;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.UI;

public partial class Settings : Page
{
    public Settings()
    {
        InitializeComponent();
        Global.Loadall();
        label.Text = "v" + Global.Toolversion;
        labebox1.Text = string.IsNullOrEmpty(Global.GamePath) ? "未选择游戏目录" : Global.GamePath;

        ArcdpslistComboBox.ItemsSource = Global.Arcdpslists;
        if (Global.Arcdpslists.Count > 0)
        {
            ArcdpslistComboBox.SelectedIndex = 0;
        }

        string arcdpsIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps.ini");
        if (File.Exists(arcdpsIni))
        {
            if (int.TryParse(Iniflie.Read("font", "size", "13", arcdpsIni), out int size))
            {
                fontSizeSlider.Value = size;
            }
        }

        if (Global.reshademode == 0)
        {
            BtnReShadeMode1.IsChecked = true;
            BtnReShadeMode2.IsChecked = false;
        }
        else if (Global.reshademode == 1)
        {
            BtnReShadeMode1.IsChecked = false;
            BtnReShadeMode2.IsChecked = true;
        }

        if (Global.installpluginmode == 0)
        {
            BtnNexus.IsChecked = false;
            BtnNormal.IsChecked = true;
            BtnTrouble.IsChecked = false;
            nowinstallmodedesc.Text = "正常模式";
        }
        else if (Global.installpluginmode == 1)
        {
            BtnNexus.IsChecked = false;
            BtnNormal.IsChecked = false;
            BtnTrouble.IsChecked = true;
            nowinstallmodedesc.Text = "疑难模式";
        }
        else if (Global.installpluginmode == 2)
        {
            BtnNexus.IsChecked = true;
            BtnNormal.IsChecked = false;
            BtnTrouble.IsChecked = false;
            nowinstallmodedesc.Text = "Nexus模式";
        }

        Task.Run(() => getwebinfomx());
        Task.Run(async () => await CheckGitHubReleaseVersionAsync());
    }

    private async Task CheckGitHubReleaseVersionAsync()
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "GW2AddonsInstallTool");
            string url = "https://api.github.com/repos/lotus8777/GW2AddonsInstallTool/releases/latest";
            HttpResponseMessage resp = await client.GetAsync(url).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                string json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tag_name", out JsonElement tagElem))
                {
                    string latestTag = tagElem.GetString() ?? "";
                    string cleanVersion = latestTag.TrimStart('v', 'V');
                    if (!string.IsNullOrEmpty(cleanVersion) && !cleanVersion.Equals(Global.Toolversion, StringComparison.OrdinalIgnoreCase))
                    {
                        Global.Toolversion_get = cleanVersion;
                        Dispatcher.Invoke(() =>
                        {
                            label1.Text = $"发现新版本 {latestTag}!";
                            updatePanel.Visibility = Visibility.Visible;
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("GitHub Release 版本检测失败", ex);
        }
    }

    public void getwebinfomx()
    {
        if (!Global.apigetok && Global.Loadall())
        {
            Global.apigetok = true;
            Task task = Task.Run(() => Getjson());
            task.Wait();

            if (task.IsCompleted)
            {
                if (Global.lastarcdpssiz == 1)
                {
                    MessageBox.Show(Global.lastarcdpsmd5, "服务器消息 - 通告");
                }
                if (Global.lastarcdpssiz == 2)
                {
                    MessageBox.Show(Global.lastarcdpsmd5, "服务器消息 - 警告");
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    nextPage.Visibility = Visibility.Hidden;
                });
            }
        }

        Dispatcher.Invoke(() =>
        {
            addons.ItemsSource = null;
            addons.ItemsSource = Global.Addons;

            ArcdpslistComboBox.ItemsSource = null;
            ArcdpslistComboBox.ItemsSource = Global.Arcdpslists;
            if (Global.Arcdpslists.Count > 0)
            {
                int index = Math.Clamp(Global.selectedarcdpsid, 0, Global.Arcdpslists.Count - 1);
                ArcdpslistComboBox.SelectedIndex = index;
            }
        });
    }

    public async Task Getjson()
    {
        using HttpClient client = new HttpClient();
        try
        {
            string url1 = "https://gitee.com/api/v5/repos/xiejunyc/gw2chajianfile/releases/latest?access_token=fd5851191878d04a089903e34ce8a003";
            HttpResponseMessage val = await client.GetAsync(url1, HttpCompletionOption.ResponseHeadersRead);
            string json = await val.Content.ReadAsStringAsync();

            try
            {
                GiteeApi giteeApi = GiteeApi.FromJson(json);
                try
                {
                    Bodyapi bodyApi = Bodyapi.FromJson(giteeApi.Body);
                    Dispatcher.Invoke(() =>
                    {
                        Global.fileday = bodyApi.Fileday;
                        Global.qqgroup = bodyApi.QQgroup;

                        if (Global.Arcdpslists.Count > 0)
                        {
                            Global.Arcdpslists[0].dps_descr = $"推荐版本 ({Global.fileday})";
                        }

                        foreach (Addon addon in Global.Addons)
                        {
                            foreach (boFile boFile2 in bodyApi.Files)
                            {
                                if (boFile2.Id == addon.Id)
                                {
                                    addon.Filename = boFile2.Name;
                                    addon.Md5st = boFile2.Md5;
                                    addon.Filesize = (int)boFile2.Size;
                                    addon.DescriptionText = boFile2.Description;
                                }
                            }
                        }
                        Global.helpinfo = bodyApi.helpinfo;
                        Global.installtoolsize = bodyApi.installtoolsize;
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError("解析 Gitee API 数据出错", ex);
                }

                Dispatcher.Invoke(() =>
                {
                    foreach (Addon addon2 in Global.Addons)
                    {
                        foreach (Asset asset in giteeApi.Assets)
                        {
                            if (!asset.BrowserDownloadUrl.Contains("0629.zip") && asset.Name.Equals(addon2.Filename, StringComparison.OrdinalIgnoreCase))
                            {
                                addon2.Urlpath = asset.BrowserDownloadUrl;
                            }
                        }
                    }
                });

                foreach (Asset asset2 in giteeApi.Assets)
                {
                    if (asset2.Name.Equals("peizi.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.Addons[15].Urlpath = asset2.BrowserDownloadUrl;
                    }
                    if (asset2.Name.Equals("d3d11.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.tuijianarcdpsurl = asset2.BrowserDownloadUrl;
                        if (Global.Arcdpslists.Count > 0)
                        {
                            Global.Arcdpslists[0].Urlpath = asset2.BrowserDownloadUrl;
                        }
                    }
                    if (asset2.Name.Equals("nexus_arcdps.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.Addons[13].Urlpath = asset2.BrowserDownloadUrl;
                    }
                    if (asset2.Name.Equals("d3d9.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.Addons[14].Urlpath = asset2.BrowserDownloadUrl;
                    }
                }
            }
            catch (Exception ex2)
            {
                Logger.LogError("解析 API 数据出错", ex2);
            }

            try
            {
                string url2 = "https://gitee.com/api/v5/repos/xiejunyc/gw2chajianfile/releases/tags/arcdpslist?access_token=fd5851191878d04a089903e34ce8a003";
                HttpResponseMessage val2 = await client.GetAsync(url2, HttpCompletionOption.ResponseHeadersRead);
                string json2 = await val2.Content.ReadAsStringAsync();
                Listbodyapi listbodyapi = Listbodyapi.FromJson(GiteeApi.FromJson(json2).Body);

                if (listbodyapi != null && listbodyapi.Files != null && listbodyapi.Files.Length > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        int num = 10;
                        foreach (LFiles file in listbodyapi.Files)
                        {
                            Arcdps item = new Arcdps(num, file.Description, file.Name, "", file.Md5, file.Size);
                            foreach (Asset asset3 in GiteeApi.FromJson(json2).Assets)
                            {
                                if (asset3.Name.Equals(item.dps_name, StringComparison.OrdinalIgnoreCase))
                                {
                                    item.Urlpath = asset3.BrowserDownloadUrl;
                                }
                            }
                            Global.Arcdpslists.Add(item);
                            num++;
                        }
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    ArcdpslistComboBox.ItemsSource = null;
                    ArcdpslistComboBox.ItemsSource = Global.Arcdpslists;
                    if (Global.Arcdpslists.Count > 0)
                    {
                        ArcdpslistComboBox.SelectedIndex = Math.Clamp(Global.selectedarcdpsid, 0, Global.Arcdpslists.Count - 1);
                    }
                });

                foreach (Asset asset4 in GiteeApi.FromJson(json).Assets)
                {
                    if (asset4.Name.Equals("d3d11.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.tuijianarcdpsurl = asset4.BrowserDownloadUrl;
                    }
                    if (asset4.Name.Equals("d3d11.zip.md5sum", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpResponseMessage val3 = await client.GetAsync(asset4.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                        Global.tuijianarcdpsmd5 = (await val3.Content.ReadAsStringAsync()).Replace("\n", "").Replace("\r", "").Replace(" ", "");
                        if (Global.Arcdpslists.Count > 0)
                        {
                            Global.Arcdpslists[0].Md5st = Global.tuijianarcdpsmd5;
                        }
                    }
                }
            }
            catch (Exception ex3)
            {
                Logger.LogError("历史 DPS 版本解析失败", ex3);
            }
        }
        catch (Exception ex4)
        {
            Logger.LogError("网络链接失败", ex4);
        }
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

    private void getpatch_button_clicked(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new OpenFolderDialog
        {
            Title = "选择激战2游戏根目录"
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = dialog.FolderName;
            if (File.Exists(Path.Combine(selectedPath, "Gw2-64.exe")) || File.Exists(Path.Combine(selectedPath, "Gw2.exe")))
            {
                Global.GamePath = selectedPath;
                labebox1.Text = Global.GamePath;
                Iniflie.Write("general", "GamePath", Global.GamePath, Iniflie.filename);
                Iniflie.Save();
                string arcdpsIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps.ini");
                if (File.Exists(arcdpsIni))
                {
                    if (int.TryParse(Iniflie.Read("font", "size", "13", arcdpsIni), out int size))
                    {
                        fontSizeSlider.Value = size;
                    }
                }
            }
            else
            {
                MessageBox.Show($"所选目录未找到 Gw2-64.exe\r\n正确路径示例: C:\\Program Files\\Guild Wars 2\r\n您选择的路径: {selectedPath}", "目录选择错误");
            }
        }
    }

    private void fontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        string arcdpsIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps.ini");
        if (File.Exists(arcdpsIni))
        {
            Iniflie.Write("font", "size", ((int)fontSizeSlider.Value).ToString(), arcdpsIni);
        }
    }

    private void RecoverDT_button_Click(object sender, RoutedEventArgs e)
    {
        string arcdpsLangIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps_lang.ini");
        if (File.Exists(arcdpsLangIni))
        {
            Iniflie.Write("lang", "703", "团队统计-", arcdpsLangIni);
            MessageBox.Show("已恢复默认标题栏", "成功");
        }
        else
        {
            MessageBox.Show("未找到 arcdps_lang.ini 配置文件", "提醒");
        }
    }

    private void uninstall_clicked_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("确定要卸载当前已安装的插件吗？", "确认卸载", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                string bin64 = Path.Combine(Global.GamePath, "bin64");
                string[] targetFiles = new string[] { "d3d9.dll", "dxgi.dll", "d3d11.dll", "addonLoader.dll", "ArcDPS.dll" };
                foreach (string f in targetFiles)
                {
                    string p1 = Path.Combine(Global.GamePath, f);
                    if (File.Exists(p1)) File.Delete(p1);
                    string p2 = Path.Combine(bin64, f);
                    if (File.Exists(p2)) File.Delete(p2);
                }
                MessageBox.Show("插件卸载完成", "卸载成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("卸载失败: " + ex.Message);
            }
        }
    }

    private void uninstall_config_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("确定要删除所有插件的配置文件吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            try
            {
                string addonsDir = Path.Combine(Global.GamePath, "addons");
                if (Directory.Exists(addonsDir))
                {
                    Directory.Delete(addonsDir, recursive: true);
                }
                MessageBox.Show("配置文件已删除", "成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败: " + ex.Message);
            }
        }
    }

    private void update_button_clicked(object sender, RoutedEventArgs e)
    {
        bool canProceed = true;
        try
        {
            Process[] processes = Process.GetProcessesByName("Gw2-64");
            foreach (Process process in processes)
            {
                canProceed = false;
                if (MessageBox.Show("检测到游戏<激战2>正在运行中...\r\n点击<是>关闭游戏程序\r\n点击<否>退出当前操作", "是否关闭游戏程序?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    process.Kill();
                    canProceed = true;
                }
                break;
            }
        }
        catch
        {
            MessageBox.Show("未知原因无法结束正在运行的激战2,\r\n请自行在任务管理器内强制结束激战2后尝试安装或更新插件", "提醒!");
        }

        if (!canProceed) return;

        if (Global.installpluginmode == 0)
        {
            Global.Addons[13].IsSelected = true;
            Global.Addons[14].IsSelected = false;
        }
        else if (Global.installpluginmode == 1)
        {
            Global.Addons[13].IsSelected = true;
            int[] array = new int[3] { 11, 12, 14 };
            foreach (int index in array)
            {
                Global.Addons[index].IsSelected = false;
            }
        }
        else if (Global.installpluginmode == 2)
        {
            int[] array = new int[8] { 1, 2, 5, 6, 8, 11, 12, 13 };
            foreach (int index2 in array)
            {
                Global.Addons[index2].IsSelected = false;
            }
            Global.Addons[14].IsSelected = true;
        }

        if (ArcdpslistComboBox.SelectedItem is Arcdps selectedArcdps)
        {
            Global.Addons[0].Md5st = selectedArcdps.Md5st;
            Global.Addons[0].Urlpath = selectedArcdps.Urlpath;
            Global.Addons[0].Filesize = selectedArcdps.Filesize;
        }
        else if (Global.Arcdpslists.Count > 0)
        {
            Arcdps defaultDps = Global.Arcdpslists[0];
            Global.Addons[0].Md5st = defaultDps.Md5st;
            Global.Addons[0].Urlpath = defaultDps.Urlpath;
            Global.Addons[0].Filesize = defaultDps.Filesize;
        }

        NavigationService?.Navigate(new Uri("UI/Install.xaml", UriKind.Relative));
    }

    private void update_self_click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/lotus8777/GW2AddonsInstallTool/releases/latest") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("打开浏览器失败: " + ex.Message);
        }
    }

    private void installmode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            if (toggleButton == BtnNexus)
            {
                Global.installpluginmode = 2;
                BtnNexus.IsChecked = true;
                BtnNormal.IsChecked = false;
                BtnTrouble.IsChecked = false;
                nowinstallmodedesc.Text = "Nexus模式";
            }
            else if (toggleButton == BtnNormal)
            {
                Global.installpluginmode = 0;
                BtnNexus.IsChecked = false;
                BtnNormal.IsChecked = true;
                BtnTrouble.IsChecked = false;
                nowinstallmodedesc.Text = "正常模式";
            }
            else if (toggleButton == BtnTrouble)
            {
                Global.installpluginmode = 1;
                BtnNexus.IsChecked = false;
                BtnNormal.IsChecked = false;
                BtnTrouble.IsChecked = true;
                nowinstallmodedesc.Text = "疑难模式";
            }
        }
        installmodeMenuPopup.IsOpen = false;
    }

    private void ReShadeMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            if (toggleButton == BtnReShadeMode1)
            {
                Global.reshademode = 0;
                BtnReShadeMode1.IsChecked = true;
                BtnReShadeMode2.IsChecked = false;
            }
            else if (toggleButton == BtnReShadeMode2)
            {
                Global.reshademode = 1;
                BtnReShadeMode1.IsChecked = false;
                BtnReShadeMode2.IsChecked = true;
            }
        }
    }

    private void installmodeMenuButton_Click(object sender, RoutedEventArgs e)
    {
        installmodeMenuPopup.IsOpen = !installmodeMenuPopup.IsOpen;
    }

    private void addOnList_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
    {
        if (addons.SelectedItem is Addon selectedAddon)
        {
            if (selectedAddon.Id == 10)
            {
                BtnReShadeMode1.Visibility = Visibility.Visible;
                BtnReShadeMode2.Visibility = Visibility.Visible;
            }
        }
    }

    private void addonschanged(object sender, RoutedEventArgs e)
    {
    }

    private void ArcdpslistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ArcdpslistComboBox.SelectedItem is Arcdps selectedArcdps)
        {
            Global.selectedarcdpsid = ArcdpslistComboBox.SelectedIndex;
            Global.selectedarcdpsname = selectedArcdps.dps_name;
            Global.selectedarcdpsurl = selectedArcdps.Urlpath;
            Global.selectedarcdpsmd5 = selectedArcdps.Md5st;
            Global.selectedarcdpssiz = selectedArcdps.Filesize;
        }
    }
}
