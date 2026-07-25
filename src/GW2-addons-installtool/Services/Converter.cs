using System.Text.Json;

namespace GW2_addons_installtool.Services;

internal static class Converter
{
    public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };
}

internal static class ConverterBodyapi
{
    public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };
}

internal static class ConverterListbodyapi
{
    public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };
}
