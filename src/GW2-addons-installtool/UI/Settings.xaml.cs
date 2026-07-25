using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
        label.Content = "工具版本: v" + Global.Toolversion;
        labebox1.Text = Global.GamePath;
        tuijiandps.Visibility = Visibility.Hidden;

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
            nowinstallmodedesc.Content = "模式: 正常模式";
        }
        else if (Global.installpluginmode == 1)
        {
            BtnNexus.IsChecked = false;
            BtnNormal.IsChecked = false;
            BtnTrouble.IsChecked = true;
            nowinstallmodedesc.Content = "模式: 疑难模式";
        }
        else if (Global.installpluginmode == 2)
        {
            BtnNexus.IsChecked = true;
            BtnNormal.IsChecked = false;
            BtnTrouble.IsChecked = false;
            nowinstallmodedesc.Content = "模式: Nexus模式";
        }

        label1.Visibility = Visibility.Hidden;
        label2.Visibility = Visibility.Hidden;
        update_self_button.Visibility = Visibility.Hidden;
        jindu.Visibility = Visibility.Hidden;

        Task.Run(() => getwebinfomx());
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
                if (string.IsNullOrEmpty(Global.Toolversion_get))
                {
                    MessageBox.Show("未获取到工具最新版本信息!您的网络可能有问题,请重启本工具再试一次");
                    Dispatcher.Invoke(() =>
                    {
                        label1.Visibility = Visibility.Hidden;
                        label2.Visibility = Visibility.Hidden;
                        update_self_button.Visibility = Visibility.Hidden;
                        jindu.Visibility = Visibility.Hidden;
                    });
                }
                else if (!Global.Toolversion_get.Equals(Global.Toolversion))
                {
                    Global.updatevisible = 1;
                    Dispatcher.Invoke(() =>
                    {
                        label.Content = "有新版本: v" + Global.Toolversion_get;
                        label1.Visibility = Visibility.Visible;
                        label2.Visibility = Visibility.Hidden;
                        update_self_button.Visibility = Visibility.Visible;
                        jindu.Visibility = Visibility.Hidden;
                        label1.Foreground = new SolidColorBrush(Colors.Red);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    if (Global.installdpsmode == 1)
                    {
                        ArcdpslistComboBox.Visibility = Visibility.Hidden;
                        tuijiandps.Visibility = Visibility.Visible;
                        labe1_1.Content = "推荐版本：" + Global.fileday;
                    }
                    else if (Global.installdpsmode == 2)
                    {
                        ArcdpslistComboBox.Visibility = Visibility.Visible;
                        tuijiandps.Visibility = Visibility.Hidden;
                    }
                });

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
                    labe1.Foreground = new SolidColorBrush(Colors.Red);
                    nextPage.Visibility = Visibility.Hidden;
                    label1.Visibility = Visibility.Hidden;
                    label2.Visibility = Visibility.Hidden;
                    update_self_button.Visibility = Visibility.Hidden;
                    jindu.Visibility = Visibility.Hidden;
                });
            }
        }

        Dispatcher.Invoke(() =>
        {
            addons.ItemsSource = Global.Addons;
            ArcdpslistComboBox.ItemsSource = Global.Arcdpslists;
            if (ArcdpslistComboBox.Items.Count > 0)
            {
                ArcdpslistComboBox.SelectedIndex = Global.selectedarcdpsid;
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
                        Global.Toolversion_get = bodyApi.Vers;
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
                    MessageBox.Show("解析获取的API数据内容出错\n\n错误原因: " + ex.Message, "解析获取的 API数据 错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("解析数据内容出错\n\n错误原因: " + ex2.Message, "解析 API数据 错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                string url2 = "https://gitee.com/api/v5/repos/xiejunyc/gw2chajianfile/releases/tags/arcdpslist?access_token=fd5851191878d04a089903e34ce8a003";
                HttpResponseMessage val2 = await client.GetAsync(url2, HttpCompletionOption.ResponseHeadersRead);
                string json2 = await val2.Content.ReadAsStringAsync();
                Listbodyapi listbodyapi = Listbodyapi.FromJson(GiteeApi.FromJson(json2).Body);

                Dispatcher.Invoke(() =>
                {
                    int num = 0;
                    Global.Arcdpslists.Clear();
                    foreach (LFiles file in listbodyapi.Files)
                    {
                        Global.Arcdpslists.Add(new Arcdps(num, file.Description, file.Name, "", file.Md5, file.Size));
                        num++;
                    }
                });

                Dispatcher.Invoke(() =>
                {
                    foreach (Arcdps arcdpslist in Global.Arcdpslists)
                    {
                        foreach (Asset asset3 in GiteeApi.FromJson(json2).Assets)
                        {
                            if (asset3.Name.Equals(arcdpslist.dps_name, StringComparison.OrdinalIgnoreCase))
                            {
                                arcdpslist.Urlpath = asset3.BrowserDownloadUrl;
                            }
                        }
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
                    }
                }

                foreach (Asset asset5 in GiteeApi.FromJson(json2).Assets)
                {
                    if (asset5.Name.Equals("d3d11.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        Global.lastarcdpsurl = asset5.BrowserDownloadUrl;
                    }
                    if (asset5.Name.Equals("d3d11.zip.md5sum", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpResponseMessage val4 = await client.GetAsync(asset5.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                        Global.lastarcdpsmd5 = (await val4.Content.ReadAsStringAsync()).Replace("\n", "").Replace("\r", "").Replace(" ", "");
                        Global.lastarcdpssiz = 1;
                    }
                }
            }
            catch (Exception ex3)
            {
                MessageBox.Show("获取历史DPS版本信息出错\n\n错误原因: " + ex3.Message, "历史DPS版本解析 错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex4)
        {
            MessageBox.Show("网络链接错误!\r\n请尝试重启本工具或稍后重试\r\n错误信息: " + ex4.Message, "网络连接 错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

    private void getpatch_button_clicked(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new OpenFolderDialog
        {
            Title = "选择激战2游戏根目录"
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = dialog.FolderName;
            if (File.Exists(Path.Combine(selectedPath, "Gw2-64.exe")))
            {
                Global.GamePath = selectedPath;
                labebox1.Text = Global.GamePath;
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
                MessageBox.Show($"所选目录路径含有中文字符或未找到 Gw2-64.exe\r\n正确路径样式:C:\\Program Files\\Guild Wars 2\r\n您选择的路径:{selectedPath}\r\n", "目录识别错误!");
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
        if (Global.updatevisible == 2)
        {
            MessageBox.Show("此功能当前不可用\r\n请耐心等待本工具更新完成!", "提醒!");
            return;
        }
        if (Global.updatevisible == 3)
        {
            MessageBox.Show("此功能当前不可用\r\n请关闭本工具,并等待20秒(后台静默更新),再重新打开本工具!", "提醒!");
            return;
        }

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

        if (Global.installdpsmode == 1)
        {
            Global.Addons[0].Md5st = Global.tuijianarcdpsmd5;
            Global.Addons[0].Urlpath = Global.tuijianarcdpsurl;
            Global.Addons[0].Filesize = Global.tuijianarcdpssiz;
        }
        else if (Global.installdpsmode == 2)
        {
            Global.Addons[0].Md5st = Global.selectedarcdpsmd5;
            Global.Addons[0].Urlpath = Global.selectedarcdpsurl;
            Global.Addons[0].Filesize = Global.selectedarcdpssiz;
        }

        NavigationService?.Navigate(new Uri("UI/Install.xaml", UriKind.Relative));
    }

    private void update_self_click(object sender, RoutedEventArgs e)
    {
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
                nowinstallmodedesc.Content = "模式: Nexus模式";
            }
            else if (toggleButton == BtnNormal)
            {
                Global.installpluginmode = 0;
                BtnNexus.IsChecked = false;
                BtnNormal.IsChecked = true;
                BtnTrouble.IsChecked = false;
                nowinstallmodedesc.Content = "模式: 正常模式";
            }
            else if (toggleButton == BtnTrouble)
            {
                Global.installpluginmode = 1;
                BtnNexus.IsChecked = false;
                BtnNormal.IsChecked = false;
                BtnTrouble.IsChecked = true;
                nowinstallmodedesc.Content = "模式: 疑难模式";
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

    private void installdpsmode_Click(object sender, RoutedEventArgs e)
    {
        if (Global.installdpsmode == 1)
        {
            Global.installdpsmode = 2;
            ArcdpslistComboBox.Visibility = Visibility.Visible;
            tuijiandps.Visibility = Visibility.Hidden;
        }
        else
        {
            Global.installdpsmode = 1;
            ArcdpslistComboBox.Visibility = Visibility.Hidden;
            tuijiandps.Visibility = Visibility.Visible;
        }
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
