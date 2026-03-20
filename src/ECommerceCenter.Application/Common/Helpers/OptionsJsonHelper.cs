using System.Text.Json;

namespace ECommerceCenter.Application.Common.Helpers;

public static class OptionsJsonHelper
{
    public static Dictionary<string, string> Parse(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson)) return [];
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(optionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
