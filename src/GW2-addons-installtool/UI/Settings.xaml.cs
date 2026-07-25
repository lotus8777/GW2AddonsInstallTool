using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using GW2_addons_installtool.Models;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.UI;

public partial class Settings : Page
{
    public Settings()
    {
        InitializeComponent();

        if (Global.GamePath == "未设置" || string.IsNullOrEmpty(Global.GamePath))
        {
            nextPage_Copy1.Visibility = Visibility.Visible;
            nextPage.Visibility = Visibility.Hidden;
            nextPage_Copy3.Visibility = Visibility.Hidden;
        }
        else
        {
            nextPage_Copy1.Visibility = Visibility.Hidden;
            nextPage.Visibility = Visibility.Visible;
            nextPage_Copy3.Visibility = Visibility.Visible;
        }

        labebox1.Text = Global.GamePath;

        if (string.IsNullOrEmpty(Global.fileday))
        {
            Global.apigetok = false;
        }
        else
        {
            descriptioninfo.Text = Global.helpinfo;
        }

        if (Global.updatevisible == 0)
        {
            label.Content = "工具版本: v" + Global.Toolversion;
            label1.Visibility = Visibility.Hidden;
            label2.Visibility = Visibility.Hidden;
            update_self_button.Visibility = Visibility.Hidden;
            jindu.Visibility = Visibility.Hidden;
        }
        else if (Global.updatevisible == 1)
        {
            label.Content = "有新版本: v" + Global.Toolversion_get;
            label1.Foreground = new SolidColorBrush(Colors.Red);
            jindu.Visibility = Visibility.Hidden;
        }

        if (Global.installpluginmode == 0)
        {
            BtnNormal.IsChecked = true;
            nowinstallmodedesc.Content = "当前: 正常模式";
        }
        else if (Global.installpluginmode == 1)
        {
            BtnTrouble.IsChecked = true;
            nowinstallmodedesc.Content = "当前: 疑难模式";
        }
        else if (Global.installpluginmode == 2)
        {
            BtnNexus.IsChecked = true;
            nowinstallmodedesc.Content = "当前: Nexus模式";
        }

        if (Global.installdpsmode == 1)
        {
            ArcdpslistComboBox.Visibility = Visibility.Hidden;
            tuijiandps.Visibility = Visibility.Visible;
            labe1_1.Content = string.IsNullOrEmpty(Global.fileday) ? "推荐版本：正在获取中..." : "推荐版本：" + Global.fileday;
        }
        else if (Global.installdpsmode == 2)
        {
            ArcdpslistComboBox.Visibility = Visibility.Visible;
            tuijiandps.Visibility = Visibility.Hidden;
        }

        if (Global.reshademode == 1)
        {
            BtnReShadeMode1.IsChecked = true;
        }
        else if (Global.reshademode == 2)
        {
            BtnReShadeMode2.IsChecked = true;
        }

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
                        descriptioninfo.Text = "未获取到工具最新版本信息!您的网络可能有问题,请重启本工具再试一次";
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

                Dispatcher.Invoke(() =>
                {
                    descriptioninfo.Text = Global.helpinfo;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    descriptioninfo.Text = "获取信息失败!请尝试重启本工具";
                    labe1.Foreground = new SolidColorBrush(Colors.Red);
                    nextPage.Visibility = Visibility.Hidden;
                    nextPage_Copy3.Visibility = Visibility.Hidden;
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

                if (Global.Addons.Count > 0)
                {
                    Global.tuijianarcdpsurl = Global.Addons[0].Urlpath;
                    Global.tuijianarcdpsmd5 = Global.Addons[0].Md5st;
                    Global.tuijianarcdpssiz = Global.Addons[0].Filesize;
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show("解析获取的API数据出错\n\n错误原因: " + ex2.Message, "解析获取的 API 错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex3)
        {
            MessageBox.Show("获取服务器信息出现错误(网络超时或Gitee API无法访问)\n\n错误原因: " + ex3.Message, "链接 Gitee API 错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        using HttpClient client2 = new HttpClient();
        try
        {
            string url2 = "https://gitee.com/api/v5/repos/jiangyi0923/arcdpslist/releases/latest?access_token=ea9e8776dbc01bd019ca905d0418c6bf";
            HttpResponseMessage val2 = await client2.GetAsync(url2, HttpCompletionOption.ResponseHeadersRead);
            string json2 = await val2.Content.ReadAsStringAsync();

            try
            {
                GiteeApi giteeApi2 = GiteeApi.FromJson(json2);
                try
                {
                    Listbodyapi bodyApi2 = Listbodyapi.FromJson(giteeApi2.Body);
                    Dispatcher.Invoke(() =>
                    {
                        Global.Arcdpslists.Clear();
                        for (int i = 0; i < bodyApi2.Files.Length; i++)
                        {
                            Global.Arcdpslists.Add(new Arcdps(i, ""));
                        }
                        foreach (Arcdps arcdpslist in Global.Arcdpslists)
                        {
                            foreach (LFiles lFiles in bodyApi2.Files)
                            {
                                if (lFiles.Id == arcdpslist.Id)
                                {
                                    arcdpslist.dps_name = lFiles.Name;
                                    arcdpslist.Md5st = lFiles.Md5;
                                    arcdpslist.Filesize = lFiles.Size;
                                    arcdpslist.dps_descr = lFiles.Description;
                                }
                            }
                        }
                    });
                }
                catch (Exception ex4)
                {
                    MessageBox.Show("解析获取的API数据内容出错\n\n错误原因1: " + ex4.Message, "解析获取的 API数据 错误1", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Dispatcher.Invoke(() =>
                {
                    foreach (Arcdps arcdpslist2 in Global.Arcdpslists)
                    {
                        foreach (Asset asset in giteeApi2.Assets)
                        {
                            if (!asset.BrowserDownloadUrl.Contains("0923.zip") && asset.Name.Equals(arcdpslist2.dps_name, StringComparison.OrdinalIgnoreCase))
                            {
                                arcdpslist2.Urlpath = asset.BrowserDownloadUrl;
                            }
                        }
                    }
                });
            }
            catch (Exception ex5)
            {
                MessageBox.Show("解析获取的API数据出错\n\n错误原因2: " + ex5.Message, "解析获取的 API 错误2", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex6)
        {
            MessageBox.Show("获取服务器信息出现错误(网络超时或Gitee API无法访问)3\n\n错误原因: " + ex6.Message, "链接 Gitee API 错误3", MessageBoxButton.OK, MessageBoxImage.Error);
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
        if (Global.updatevisible == 3)
        {
            try
            {
                Process.Start("Updater.exe");
            }
            catch { }
        }
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
        MessageBox.Show("空中网,美服客户端请选择bin64文件夹的上级目录\r\n不是喊你选bin64文件夹!!!!\r\n不是喊你选bin64文件夹!!!!\r\n不是喊你选bin64文件夹!!!!\r\nWG请选择\"Guild_Wars_2(2001128)\"类似目录\r\n心游请选择\"心游登录器\"所在位置的\"data\"目录");

        OpenFolderDialog dialog = new OpenFolderDialog
        {
            Title = "选择激战2游戏根目录"
        };

        if (dialog.ShowDialog() == true)
        {
            string selectedPath = dialog.FolderName;
            bool hasNonAscii = false;
            foreach (char c in selectedPath)
            {
                if (c > 127)
                {
                    hasNonAscii = true;
                    break;
                }
            }

            if (!hasNonAscii)
            {
                if (!File.Exists(Path.Combine(selectedPath, "Gw2-64.exe")))
                {
                    MessageBox.Show($"所选目录路径未发现Gw2-64.exe文件\r\n您选择的路径:{selectedPath}\r\n", "未发现Gw2-64.exe");
                    return;
                }
                Global.GamePath = selectedPath;
                labebox1.Text = Global.GamePath;
                nextPage_Copy1.Visibility = Visibility.Hidden;
                nextPage.Visibility = Visibility.Visible;
                nextPage_Copy3.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show($"所选目录路径含有中文字符,请更改后尝试\r\n正确路径样式:C:\\Program Files\\Guild Wars 2\r\n您选择的路径:{selectedPath}\r\n", "目录识别错误!");
            }
        }
    }

    private void getpatch_button_clicked2(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://docs.qq.com/doc/p/6dc57d3768b5fb2d99347210df532d35be24cd00") { UseShellExecute = true });
        }
        catch { }
    }

    private void options_button_clicked(object sender, RoutedEventArgs e)
    {
        if (Global.updatevisible == 2)
        {
            MessageBox.Show("此功能当前不可用\r\n请耐心等待本工具更新完成!", "提醒!");
        }
        else if (Global.updatevisible == 3)
        {
            MessageBox.Show("此功能当前不可用\r\n请关闭本工具,并等待20秒(后台静默更新),再重新打开本工具!", "提醒!");
        }
        else
        {
            NavigationService?.Navigate(new Uri("UI/Options.xaml", UriKind.Relative));
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
            MessageBox.Show("请勿在游戏内点击插件弹出的更新提示按钮！！！！！！\r\n\r\n为了更高的兼容性,请您确认您的游戏已设置：窗口全屏模式\r\n\r\n如果安装无效请选择其他安装模式", "安装提醒!");
        }
        else if (Global.installpluginmode == 1)
        {
            Global.Addons[13].IsSelected = true;
            int[] array = new int[3] { 11, 12, 14 };
            foreach (int index in array)
            {
                Global.Addons[index].IsSelected = false;
            }
            MessageBox.Show("请勿在游戏内点击插件弹出的更新提示按钮！！！！！！\r\n\r\n为了更高的兼容性,请您确认您的游戏已设置：窗口全屏模式\r\n\r\n滤镜模式一安装ReShade滤镜可能无效\r\n\r\n滤镜无效时可尝试选择滤镜模式二", "安装提醒!");
        }
        else if (Global.installpluginmode == 2)
        {
            int[] array = new int[8] { 1, 2, 5, 6, 8, 11, 12, 13 };
            foreach (int index2 in array)
            {
                Global.Addons[index2].IsSelected = false;
            }
            Global.Addons[14].IsSelected = true;
            MessageBox.Show("请勿在游戏内点击插件弹出的更新提示按钮！！！！！！\r\n\r\n为了更高的兼容性,请您确认您的游戏已设置：窗口全屏模式\r\n\r\n滤镜模式一安装ReShade滤镜可能无效\r\n\r\n滤镜无效时可尝试选择滤镜模式二", "安装提醒!");
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

    private void addOnList_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
    {
        if (addons.SelectedIndex >= 0 && addons.SelectedIndex < Global.Addons.Count)
        {
            Addon selected = Global.Addons[addons.SelectedIndex];
            descriptioninfo.Text = selected.DescriptionText;
        }
    }

    private void addonschanged(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is Addon addon)
        {
            // Update addon selections
        }
    }

    private void ArcdpslistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        descriptioninfo.Text = Global.helpinfo;
        if (ArcdpslistComboBox.SelectedItem is Arcdps arcdps)
        {
            Global.selectedarcdpsid = arcdps.Id;
            Global.selectedarcdpsurl = arcdps.Urlpath;
            Global.selectedarcdpsmd5 = arcdps.Md5st;
            Global.selectedarcdpssiz = arcdps.Filesize;
        }
    }

    private void installmodeMenuButton_Click(object sender, RoutedEventArgs e)
    {
        installmodeMenuPopup.IsOpen = !installmodeMenuPopup.IsOpen;
    }

    private void installmode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            BtnNormal.IsChecked = false;
            BtnTrouble.IsChecked = false;
            BtnNexus.IsChecked = false;
            toggleButton.IsChecked = true;

            if (toggleButton == BtnNormal)
            {
                Global.installpluginmode = 0;
                descriptioninfo.Text = "正常模式: \r\n此模式已停止更新维护!建议使用Nexus模式!";
                nowinstallmodedesc.Content = "当前: 正常模式";
                Global.Addons[13].IsSelected = true;
                Global.Addons[14].IsSelected = false;
            }
            else if (toggleButton == BtnTrouble)
            {
                Global.installpluginmode = 1;
                descriptioninfo.Text = "疑难模式: \r\n此模式已停止更新维护!建议使用Nexus模式!";
                nowinstallmodedesc.Content = "当前: 疑难模式";
                Global.Addons[13].IsSelected = true;
                Global.Addons[14].IsSelected = false;
            }
            else if (toggleButton == BtnNexus)
            {
                Global.installpluginmode = 2;
                descriptioninfo.Text = "Nexus模式: \r\n强烈推荐模式!支持热加载，热卸载，热更新!";
                nowinstallmodedesc.Content = "当前: Nexus模式";
                Global.Addons[13].IsSelected = false;
                Global.Addons[14].IsSelected = true;
            }
            addons.Items.Refresh();
            installmodeMenuPopup.IsOpen = false;
        }
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
            labe1_1.Content = "推荐版本：" + Global.fileday;
        }
    }

    private void ReShadeMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            BtnReShadeMode1.IsChecked = false;
            BtnReShadeMode2.IsChecked = false;
            toggleButton.IsChecked = true;

            if (toggleButton == BtnReShadeMode1)
            {
                Global.reshademode = 1;
            }
            else
            {
                Global.reshademode = 2;
            }
        }
    }

    private void update_self_click(object sender, RoutedEventArgs e)
    {
        // Self update action
    }
}
