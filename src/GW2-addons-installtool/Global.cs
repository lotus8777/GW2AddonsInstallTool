using System.Collections.Generic;
using GW2_addons_installtool.Models;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool;

internal class Global
{
    public static string Toolversion = "3.2.8";
    public static string Toolversion_get = string.Empty;
    public static string GamePath = string.Empty;
    public static List<Addon> Addons = new List<Addon>();
    public static List<Arcdps> Arcdpslists = new List<Arcdps>();

    public static string fileday = "";
    public static string qqgroup = "";
    public static int y;
    public static int m;
    public static int d;
    public static string vcv1 = string.Empty;
    public static string vcv2 = string.Empty;
    public static string vcvs = "未获取到";
    public static int updatevisible = 0;
    public static int installpluginmode;
    public static int installdpsmode;
    public static int reshademode;
    public static bool apigetok = false;
    public static bool installlast = false;
    public static string lastarcdpsurl = "https://gitee.com/jiangyi0923/gw2chajianfile/releases/download/0629/d3d9_last.zip";
    public static string helpinfo = "";
    public static long installtoolsize;
    public static string lastarcdpsmd5 = "";
    public static int lastarcdpssiz;

    public static bool isselectedarcdps = false;
    public static int selectedarcdpsid = 0;
    public static string selectedarcdpsname = "";
    public static string selectedarcdpsurl = "";
    public static string selectedarcdpsmd5 = "";
    public static int selectedarcdpssiz;

    public static string tuijianarcdpsurl = "";
    public static string tuijianarcdpsmd5 = "";
    public static int tuijianarcdpssiz;

    public static bool Loadall()
    {
        if (Addons.Count == 0)
        {
            Addons.Add(new Addon(0, "ARCDPS插件", _IsSelected: true, ""));
            Addons.Add(new Addon(1, "SCT流动输出", _IsSelected: false, ""));
            Addons.Add(new Addon(2, "团队机制插件", _IsSelected: false, ""));
            Addons.Add(new Addon(3, "团队增益插件", _IsSelected: false, ""));
            Addons.Add(new Addon(4, "团队治疗统计", _IsSelected: false, ""));
            Addons.Add(new Addon(5, "配装板插件", _IsSelected: false, ""));
            Addons.Add(new Addon(6, "神油工具插件", _IsSelected: false, ""));
            Addons.Add(new Addon(7, "神油坐骑插件", _IsSelected: false, ""));
            Addons.Add(new Addon(8, "配方搜索插件", _IsSelected: false, ""));
            Addons.Add(new Addon(9, "退团监控插件", _IsSelected: false, ""));
            Addons.Add(new Addon(10, "ReShade滤镜", _IsSelected: false, ""));
            Addons.Add(new Addon(11, "神油工具插件(独立版)", _IsSelected: false, ""));
            Addons.Add(new Addon(12, "自定义增益ui插件", _IsSelected: false, ""));
            Addons.Add(new Addon(97, "ARCDPS插件包装", _IsSelected: true, ""));
            Addons.Add(new Addon(98, "Nexus核心文件", _IsSelected: true, ""));
            Addons.Add(new Addon(99, "配置文件", _IsSelected: true, ""));
        }

        foreach (Addon addon in Addons)
        {
            if (bool.TryParse(Iniflie.Read("general", addon.Addon_name, addon.IsSelected.ToString(), Iniflie.filename), out bool isSelected))
            {
                addon.IsSelected = isSelected;
            }
        }
        return true;
    }
}
