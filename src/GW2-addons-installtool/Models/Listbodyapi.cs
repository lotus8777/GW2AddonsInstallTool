using System;
using Newtonsoft.Json;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class Listbodyapi
{
    [JsonProperty("lfiles")]
    public LFiles[] Files { get; set; } = Array.Empty<LFiles>();

    public static Listbodyapi FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Listbodyapi>(json, ConverterListbodyapi.Settings) ?? new Listbodyapi();
    }
}
