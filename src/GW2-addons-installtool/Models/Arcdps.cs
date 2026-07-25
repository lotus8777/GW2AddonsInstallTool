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

    public Arcdps(int _id, string _dps_name)
    {
        Id = _id;
        dps_name = _dps_name;
    }
}
