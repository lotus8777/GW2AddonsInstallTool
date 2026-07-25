using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.UI;

public partial class Options : Page
{
    public Options()
    {
        InitializeComponent();
        label4.Content = string.IsNullOrEmpty(Global.qqgroup) ? "未获取到" : Global.qqgroup;

        string arcdpsIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps.ini");
        if (File.Exists(arcdpsIni))
        {
            if (int.TryParse(Iniflie.Read("font", "size", "13", arcdpsIni), out int size))
            {
                slider.Value = size;
            }
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

    private void back_clicked(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new Uri("UI/Settings.xaml", UriKind.Relative));
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
                MessageBox.Show($"已修改游戏路径为: {Global.GamePath}");
            }
            else
            {
                MessageBox.Show("所选目录未找到 Gw2-64.exe");
            }
        }
    }

    private void slider_valueCG(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        string arcdpsIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps.ini");
        if (File.Exists(arcdpsIni))
        {
            Iniflie.Write("font", "size", ((int)slider.Value).ToString(), arcdpsIni);
        }
    }

    private void uninstall_clicked(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("确定要卸载当前已安装的插件吗？", "确认卸载", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            try
            {
                string bin64 = Path.Combine(Global.GamePath, "bin64");
                string addonsDir = Path.Combine(Global.GamePath, "addons");

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

    private void uninstall_clicked1(object sender, RoutedEventArgs e)
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

    private void RecoverDT_button(object sender, RoutedEventArgs e)
    {
        string arcdpsLangIni = Path.Combine(Global.GamePath, @"addons\arcdps\arcdps_lang.ini");
        if (File.Exists(arcdpsLangIni))
        {
            Iniflie.Write("lang", "703", "团队统计-", arcdpsLangIni);
            MessageBox.Show("已恢复默认标题栏", "成功");
        }
    }
}
