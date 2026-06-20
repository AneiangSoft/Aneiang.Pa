using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Aneiang.Pa.Core.Mapping;

/// <summary>把 ScrapeResult 的字段字典映射为强类型 T</summary>
public static class FieldMapper
{
    /// <summary>映射单条字典到 T</summary>
    public static T Map<T>(IReadOnlyDictionary<string, string?> fields) where T : class, new()
    {
        var instance = new T();
        var type = typeof(T);
        foreach (var kv in fields)
        {
            var prop = type.GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanWrite) continue;
            try
            {
                var value = ConvertTo(kv.Value, prop.PropertyType);
                prop.SetValue(instance, value);
            }
            catch
            {
                // 单个字段失败不影响整体
            }
        }
        return instance;
    }

    private static object? ConvertTo(string? raw, Type targetType)
    {
        if (raw == null)
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                ? Activator.CreateInstance(targetType)
                : null;

        var t = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (t == typeof(string)) return raw;
        if (t == typeof(int)) return int.Parse(raw);
        if (t == typeof(long)) return long.Parse(raw);
        if (t == typeof(double)) return double.Parse(raw);
        if (t == typeof(bool)) return bool.Parse(raw);
        if (t == typeof(DateTime)) return DateTime.Parse(raw);
        // 兜底 JSON 反序列化
        return JsonSerializer.Deserialize(raw, t);
    }
}
