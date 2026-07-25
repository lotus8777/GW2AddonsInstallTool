using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class Listbodyapi
{
    [JsonPropertyName("lfiles")]
    public LFiles[] Files { get; set; } = Array.Empty<LFiles>();

    public static Listbodyapi FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Listbodyapi();
        return JsonSerializer.Deserialize<Listbodyapi>(json, ConverterListbodyapi.Settings) ?? new Listbodyapi();
    }
}
