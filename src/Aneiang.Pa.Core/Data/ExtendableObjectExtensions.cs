using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Aneiang.Pa.Core.Data
{
    /// <summary>
    /// 扩展字段扩展类
    /// </summary>
    public static class ExtendableObjectExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        /// <summary>
        /// 设置源数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendableObject"></param>
        /// <param name="value"></param>
        public static void SetOriginal<T>(this IExtendableObject extendableObject, T value)
        {
            extendableObject.SetProperty("Original", value);
        }

        /// <summary>
        /// 获取源数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendableObject"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetOriginal<T>(this IExtendableObject extendableObject, T value)
        {
            return extendableObject.GetProperty<T>("Original");
        }

        /// <summary>
        /// 设置扩展字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendableObject"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void SetProperty<T>(this IExtendableObject extendableObject, string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("键不能为空", nameof(key));

            try
            {
                var dict = new Dictionary<string, JsonElement>();
                if (!string.IsNullOrWhiteSpace(extendableObject.ExtensionData))
                {
                    using var doc = JsonDocument.Parse(extendableObject.ExtensionData);
                    var root = doc.RootElement;
                    dict = root.Deserialize<Dictionary<string, JsonElement>>(Options)
                               ?? new Dictionary<string, JsonElement>();
                }

                var newValueJson = JsonSerializer.Serialize(value, Options);
                var newElement = JsonSerializer.Deserialize<JsonElement>(newValueJson, Options);

                dict[key] = newElement;

                extendableObject.ExtensionData = JsonSerializer.Serialize(dict, Options);
            }
            catch
            {
                var dict = new Dictionary<string, JsonElement>();
                var newValueJson = JsonSerializer.Serialize(value, Options);
                var newElement = JsonSerializer.Deserialize<JsonElement>(newValueJson, Options);
                dict[key] = newElement;
                extendableObject.ExtensionData = JsonSerializer.Serialize(dict, Options);
            }
        }

        /// <summary>
        /// 获取扩展字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extendableObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetProperty<T>(this IExtendableObject extendableObject, string key)
        {
            try
            {
                using var doc = JsonDocument.Parse(extendableObject.ExtensionData);
                var root = doc.RootElement;

                if (root.TryGetProperty(key, out var property))
                {
                    return property.Deserialize<T>(Options);
                }

                return default;
            }
            catch
            {
                return default;
            }
        }
    }
}
