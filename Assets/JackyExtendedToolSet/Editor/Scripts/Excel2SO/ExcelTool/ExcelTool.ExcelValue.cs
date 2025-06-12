using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using UnityEngine;

namespace JackyExtendedToolSet.Editor.Excel2SO
{
    public static partial class ExcelTool
    {
        // 填充对象数据
        private static void FillObjectData(
            object obj, Type type, IXLWorksheet worksheet, int rowNum, List<string> fieldNames, List<string> dataTypes, List<string> separators
        )
        {
            for (var i = 0; i < fieldNames.Count; i++)
            {
                var fieldName = fieldNames[i];
                var dataTypeName = dataTypes[i];
                if (string.IsNullOrEmpty(fieldName))
                {
                    if (dataTypeName.Contains("notation"))
                    {
                        continue;
                    }
                }

                var cellValue = worksheet.Cell(rowNum, i + 1).GetValue<string>() ?? "";
                var separator = i < separators.Count ? separators[i] : ",";

                var field = type.GetField(fieldName);
                if (field == null) continue;

                try
                {
                    var value = ConvertValue(cellValue, dataTypeName, separator);
                    field.SetValue(obj, value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ExcelTools.ExcelValue:35>1: {fieldName},2: {dataTypeName},3: {cellValue},4: {separator}");
                    throw;
                }
            }
        }

        // 转换值类型
        private static object ConvertValue(string value, string dataType, string separator)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetDefaultValue(dataType);
            }

            var baseType = dataType.Replace("[]", "");

            if (dataType.EndsWith("[]"))
            {
                var elements = value.Split(new[] { separator }, StringSplitOptions.None);
                var type = MapDataType(baseType);
                if (type != null)
                {
                    var listType = typeof(List<>).MakeGenericType(type);
                    var lst = (IList)Activator.CreateInstance(listType);

                    foreach (var t in elements)
                    {
                        lst.Add(ConvertSingleValue(t.Trim(), baseType));
                    }

                    return lst;
                }

                Debug.LogWarning($"未知数据类型: {dataType}，使用默认值");
                return null;
            }

            return ConvertSingleValue(value, dataType);
        }

        // 转换单个值
        private static object ConvertSingleValue(string value, string dataType)
        {
            switch (dataType.ToLower())
            {
                case "int":
                    return int.TryParse(value, out var intVal) ? intVal : 0;
                case "float":
                    return float.TryParse(value, out var floatVal) ? floatVal : 0f;
                case "double":
                    return double.TryParse(value, out var doubleVal) ? doubleVal : 0.0;
                case "bool":
                    return bool.TryParse(value, out var boolVal) && boolVal;
                case "string":
                    return value;
                case "vector2":
                    return ParseVector2(value);
                case "vector3":
                    return ParseVector3(value);
                case "vector4":
                    return ParseVector4(value);
                case "color":
                    return ParseColor(value);
                default:
                    // 尝试查找枚举类型
                    var enumType = Type.GetType(dataType);
                    if (enumType is { IsEnum: true })
                    {
                        try
                        {
                            return Enum.Parse(enumType, value);
                        }
                        catch
                        {
                            return Enum.GetValues(enumType).GetValue(0);
                        }
                    }

                    Debug.LogWarning($"未知数据类型: {dataType}，使用默认值");
                    return null;
            }
        }

        // 获取默认值
        private static object GetDefaultValue(string dataType)
        {
            if (dataType.EndsWith("[]"))
            {
                var baseType = dataType.Replace("[]", "");
                var type = MapDataType(baseType);
                var listType = typeof(List<>).MakeGenericType(type);
                return (IList)Activator.CreateInstance(listType);
            }

            return dataType.ToLower() switch
            {
                "int" => 0,
                "float" => 0f,
                "double" => 0.0,
                "bool" => false,
                "string" => "",
                "vector2" => Vector2.zero,
                "vector3" => Vector3.zero,
                "vector4" => Vector4.zero,
                "color" => Color.white,
                _ => null
            };
        }

        // 解析Vector2
        private static Vector2 ParseVector2(string value)
        {
            var parts = Regex.Split(value.Trim('(', ')'), @"\s*,\s*");
            if (
                parts.Length >= 2 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y)
            )
            {
                return new Vector2(x, y);
            }

            return Vector2.zero;
        }

        // 解析Vector3
        private static Vector3 ParseVector3(string value)
        {
            var parts = Regex.Split(value.Trim('(', ')'), @"\s*,\s*");
            if (
                parts.Length >= 3 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y) &&
                float.TryParse(parts[2], out var z)
            )
            {
                return new Vector3(x, y, z);
            }

            return Vector3.zero;
        }

        // 解析Vector4
        private static Vector4 ParseVector4(string value)
        {
            var parts = Regex.Split(value.Trim('(', ')'), @"\s*,\s*");
            if (
                parts.Length >= 4 &&
                float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y) &&
                float.TryParse(parts[2], out var z) &&
                float.TryParse(parts[3], out var w)
            )
            {
                return new Vector4(x, y, z, w);
            }

            return Vector4.zero;
        }

        // 解析Color
        private static Color ParseColor(string value)
        {
            // 支持格式: #RRGGBB, #RRGGBBAA, RGB(r,g,b), RGBA(r,g,b,a)
            if (value.StartsWith("#"))
            {
                value = value.TrimStart('#');
                if (
                    value.Length == 6 && ColorUtility.TryParseHtmlString("#" + value, out Color color)
                )
                {
                    return color;
                }

                if (value.Length != 8) return Color.white;

                var r = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
                var g = byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber);
                var b = byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber);
                var a = byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }

            if (value.StartsWith("RGB(") || value.StartsWith("RGBA("))
            {
                var hasAlpha = value.StartsWith("RGBA(");
                var parts = Regex.Split(
                    value.TrimStart("RGB(".ToCharArray()).TrimEnd(')'),
                    @"\s*,\s*"
                );

                if ((hasAlpha && parts.Length >= 4) || (!hasAlpha && parts.Length >= 3))
                {
                    var a = hasAlpha ? float.Parse(parts[3]) : 1f;
                    if (
                        float.TryParse(parts[0], out var r) &&
                        float.TryParse(parts[1], out var g) &&
                        float.TryParse(parts[2], out var b)
                    )
                    {
                        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                    }
                }
            }

            return Color.white;
        }

        // 映射数据类型
        private static string MapDataTypeToString(string excelType)
        {
            switch (excelType.ToLower())
            {
                case "int": return "int";
                case "float": return "float";
                case "double": return "double";
                case "bool": return "bool";
                case "string": return "string";
                case "vector2": return "Vector2";
                case "vector3": return "Vector3";
                case "vector4": return "Vector4";
                case "color": return "Color";
                case "int[]": return "List<int>";
                case "string[]": return "List<string>";
                case "float[]": return "List<float>";
                case "double[]": return "List<double>";
                default:
                {
                    // 检查是否为枚举类型
                    var enumType = Type.GetType(excelType);
                    if (enumType is { IsEnum: true })
                    {
                        return excelType;
                    }

                    Debug.LogError($"ExcelTools.ExcelValue:283>未知数据类型: {excelType}");
                    return "string"; // 默认当作字符串处理
                }
            }
        }

        // 映射数据类型
        private static Type MapDataType(string excelType)
        {
            switch (excelType.ToLower())
            {
                case "int": return typeof(int);
                case "float": return typeof(float);
                case "double": return typeof(double);
                case "bool": return typeof(bool);
                case "string": return typeof(string);
                case "vector2": return Type.GetType("UnityEngine.Vector2,UnityEngine");
                case "vector3": return Type.GetType("UnityEngine.Vector3,UnityEngine");
                case "vector4": return Type.GetType("UnityEngine.Vector4,UnityEngine");
                case "color": return Type.GetType("UnityEngine.Color,UnityEngine");
                case "int[]": return Type.GetType("System.Collections.Generic.List<int>");
                case "string[]": return Type.GetType("System.Collections.Generic.List<string>");
                case "float[]": return Type.GetType("System.Collections.Generic.List<float>");
                case "double[]": return Type.GetType("System.Collections.Generic.List<double>");
                default:
                {
                    // 检查是否为枚举类型
                    var enumType = Type.GetType(excelType);
                    if (enumType is { IsEnum: true })
                    {
                        return enumType;
                    }

                    Debug.LogError($"ExcelTools.ExcelValue:315>未知数据类型: {excelType}");
                    return typeof(string); // 默认当作字符串处理
                }
            }
        }
    }
}