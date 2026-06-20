using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aneiang.Pa.Core.Data
{
    /// <summary>
    /// 扩展字段扩展类（基于 JsonNode 增量修改，避免全量序列化）
    /// </summary>
    public static class ExtendableObjectExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        /// <summary>
        /// 设置源数据
        /// </summary>
        public static void SetOriginal<T>(this IExtendableObject extendableObject, T value)
        {
            extendableObject.SetProperty("Original", value);
        }

        /// <summary>
        /// 获取源数据
        /// </summary>
        public static T? GetOriginal<T>(this IExtendableObject extendableObject)
        {
            return extendableObject.GetProperty<T>("Original");
        }

        /// <summary>
        /// 获取源数据（兼容旧签名）
        /// </summary>
        [Obsolete("使用 GetOriginal<T>() 无参重载替代")]
        public static T? GetOriginal<T>(this IExtendableObject extendableObject, T value)
        {
            return extendableObject.GetProperty<T>("Original");
        }

        /// <summary>
        /// 设置扩展字段（基于 JsonNode 增量修改，O(扩展字段数)）
        /// </summary>
        public static void SetProperty<T>(this IExtendableObject extendableObject, string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("键不能为空", nameof(key));

            JsonObject root;
            if (!string.IsNullOrWhiteSpace(extendableObject.ExtensionData))
            {
                try
                {
                    var parsed = JsonNode.Parse(extendableObject.ExtensionData);
                    root = parsed as JsonObject ?? new JsonObject();
                }
                catch
                {
                    root = new JsonObject();
                }
            }
            else
            {
                root = new JsonObject();
            }

            root[key] = value == null
                ? null
                : JsonSerializer.SerializeToNode(value, Options);

            extendableObject.ExtensionData = root.ToJsonString(Options);
        }

        /// <summary>
        /// 获取扩展字段（O(扩展字段数)）
        /// </summary>
        public static T? GetProperty<T>(this IExtendableObject extendableObject, string key)
        {
            if (string.IsNullOrWhiteSpace(extendableObject.ExtensionData))
                return default;

            try
            {
                var parsed = JsonNode.Parse(extendableObject.ExtensionData);
                if (parsed is not JsonObject root)
                    return default;

                if (!root.TryGetPropertyValue(key, out var node) || node == null)
                    return default;

                return node.Deserialize<T>(Options);
            }
            catch
            {
                return default;
            }
        }
    }
}
