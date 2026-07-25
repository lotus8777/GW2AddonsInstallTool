using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class GiteeApi
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("assets")]
    public Asset[] Assets { get; set; } = Array.Empty<Asset>();

    public static GiteeApi FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new GiteeApi();
        return JsonSerializer.Deserialize<GiteeApi>(json, Converter.Settings) ?? new GiteeApi();
    }
}
