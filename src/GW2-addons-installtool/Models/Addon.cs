using System;

namespace GW2_addons_installtool.Models;

[Serializable]
public class Addon
{
    public int Id { get; set; }
    public string Addon_name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public string Tooltipst { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string Md5st { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public string Urlpath { get; set; } = string.Empty;
    public int Filesize { get; set; }
    public int Downloadsize { get; set; }
    public bool IS_in_Download { get; set; }
    public bool Download_Down { get; set; }
    public bool Download_Have_err { get; set; }
    public bool Install_Down { get; set; }
    public bool Install_Have_err { get; set; }

    public Addon(int _id, string _Addon_name, bool _IsSelected, string _Tooltipst)
    {
        Id = _id;
        Addon_name = _Addon_name;
        IsSelected = _IsSelected;
        Tooltipst = _Tooltipst;
    }
}
