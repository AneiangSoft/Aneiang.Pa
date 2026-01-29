using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Aneiang.Pa.Dynamic.Extensions
{
    public static class PropertyInfoExtensions
    {

        /// <summary>
        /// 动态设置Value属性值，支持基本类型的字符串转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="instance"></param>
        /// <param name="stringValue"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetPropertyValue<T>(this PropertyInfo property, T instance, string stringValue)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            object convertedValue = ConvertStringToPropertyType(stringValue, property.PropertyType);
            property.SetValue(instance, convertedValue);
        }

        /// <summary>
        /// 通用的字符串到类型转换方法
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        private static object ConvertStringToPropertyType(string stringValue, Type targetType)
        {
            if (targetType == typeof(string))
                return stringValue;

            if (string.IsNullOrEmpty(stringValue))
            {
                Type nullableType = Nullable.GetUnderlyingType(targetType);
                if (nullableType != null)
                    return null;

                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);

                return null;
            }

            try
            {
                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (underlyingType == typeof(int))
                    return int.Parse(stringValue);
                if (underlyingType == typeof(long))
                    return long.Parse(stringValue);
                if (underlyingType == typeof(bool))
                {
                    if (stringValue.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (stringValue.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("no", StringComparison.OrdinalIgnoreCase))
                        return false;
                    return bool.Parse(stringValue);
                }
                if (underlyingType == typeof(decimal))
                    return decimal.Parse(stringValue);
                if (underlyingType == typeof(double))
                    return double.Parse(stringValue);
                if (underlyingType == typeof(float))
                    return float.Parse(stringValue);
                if (underlyingType == typeof(DateTime))
                    return DateTime.Parse(stringValue);
                if (underlyingType == typeof(Guid))
                    return Guid.Parse(stringValue);
                if (underlyingType == typeof(byte))
                    return byte.Parse(stringValue);
                if (underlyingType == typeof(short))
                    return short.Parse(stringValue);
                if (underlyingType == typeof(char))
                    return stringValue[0];
                if (underlyingType.IsEnum)
                    return Enum.Parse(underlyingType, stringValue, true);

                var converter = TypeDescriptor.GetConverter(underlyingType);
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return converter.ConvertFromString(stringValue);
                }
                return Convert.ChangeType(stringValue, underlyingType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"无法将字符串 '{stringValue}' 转换为类型 {targetType.Name}。详情：{ex.Message}", ex);
            }
        }
    }
}
