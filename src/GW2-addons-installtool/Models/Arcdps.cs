using System;

namespace GW2_addons_installtool.Models;

[Serializable]
public class Arcdps
{
    public int Id { get; set; }
    public string dps_name { get; set; } = string.Empty;
    public string Md5st { get; set; } = string.Empty;
    public string Urlpath { get; set; } = string.Empty;
    public int Filesize { get; set; }
    public string dps_descr { get; set; } = string.Empty;

    public string DisplayText => string.IsNullOrWhiteSpace(dps_descr) ? dps_name : $"{dps_descr} ({dps_name})";

    public Arcdps(int _id, string _dps_name)
    {
        Id = _id;
        dps_name = _dps_name;
    }

    public Arcdps(int _id, string _dps_descr, string _dps_name, string _urlpath, string _md5, int _size)
    {
        Id = _id;
        dps_descr = _dps_descr;
        dps_name = _dps_name;
        Urlpath = _urlpath;
        Md5st = _md5;
        Filesize = _size;
    }

    public override string ToString() => DisplayText;
}
