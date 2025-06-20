using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// Helper class for JSON parsing operations
/// </summary>
public static class JsonHelper
{
    public static string GetStringValue(Dictionary<string, object> jsonResponse, string key, string defaultValue = "")
    {
        if (jsonResponse.TryGetValue(key, out var value))
        {
            if (value is JsonElement element)
            {
                return element.GetString() ?? defaultValue;
            }
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }

    public static List<string> GetStringArrayValue(Dictionary<string, object> jsonResponse, string key)
    {
        if (jsonResponse.TryGetValue(key, out var value))
        {
            if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray()
                    .Select(item => item.GetString())
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToList();
            }
        }
        return new List<string>();
    }

    public static double GetDoubleValue(Dictionary<string, object> jsonResponse, string key, double defaultValue = 0.0)
    {
        if (jsonResponse.TryGetValue(key, out var value))
        {
            if (value is JsonElement element)
            {
                if (element.TryGetDouble(out var doubleValue))
                {
                    return doubleValue;
                }
            }
            else if (double.TryParse(value?.ToString(), out var parsedValue))
            {
                return parsedValue;
            }
        }
        return defaultValue;
    }

    public static int GetIntValue(Dictionary<string, object> jsonResponse, string key, int defaultValue = 0)
    {
        if (jsonResponse.TryGetValue(key, out var value))
        {
            if (value is JsonElement element)
            {
                if (element.TryGetInt32(out var intValue))
                {
                    return intValue;
                }
            }
            else if (int.TryParse(value?.ToString(), out var parsedValue))
            {
                return parsedValue;
            }
        }
        return defaultValue;
    }

    public static bool GetBoolValue(Dictionary<string, object> jsonResponse, string key, bool defaultValue = false)
    {
        if (jsonResponse.TryGetValue(key, out var value))
        {
            if (value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.True)
                    return true;
                if (element.ValueKind == JsonValueKind.False)
                    return false;
            }
            else if (bool.TryParse(value?.ToString(), out var parsedValue))
            {
                return parsedValue;
            }
        }
        return defaultValue;
    }
}
