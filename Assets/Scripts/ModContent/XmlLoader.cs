using System;
using System.IO;
using System.Xml;
using UnityEngine;
using ModContent.XmlConverter;
using ModContent.XmlExtendedAttribute;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Static class để deserialize XML thành C# objects.
/// Hỗ trợ: primitive types, classes, structs, collections (List<T>), polymorphism.
/// 
/// Features:
/// - Auto type resolution từ XML Class attribute
/// - Attribute processing (GameObjectTarget, Class, Condition)
/// - Nested object deserialization
/// - Collection support (List<T>)
/// - Error handling với configurable logging
/// </summary>
public static class XmlLoader
{
    /// <summary>
    /// State key cho condition result từ ConditionalAttributeProcessor.
    /// </summary>
    private const string StateKeyConditionResult = ConditionalAttributeProcessor.StateKeyConditionResult;

    /// <summary>
    /// Cached MethodInfo cho DeserializeFromXml (tránh reflection lookup mỗi lần).
    /// </summary>
    private static readonly MethodInfo _deserializeMethod = typeof(XmlLoader)
        .GetMethod("DeserializeFromXml", BindingFlags.Public | BindingFlags.Static);

    /// <summary>
    /// Cache generic method theo elementType (key = elementType, value = MethodInfo).
    /// </summary>
    private static readonly Dictionary<Type, MethodInfo> _genericDeserializeCache = new Dictionary<Type, MethodInfo>();

    /// <summary>
    /// Cache generic method cho List.Add theo elementType.
    /// </summary>
    private static readonly Dictionary<Type, MethodInfo> _listAddMethodCache = new Dictionary<Type, MethodInfo>();

    /// <summary>
    /// Đọc file XML và deserialize thành object type T.
    /// </summary>
    public static T LoadFromXml<T>(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found at path: {path}");
            return default;
        }

        string xmlContent = File.ReadAllText(path);
        if (string.IsNullOrEmpty(xmlContent))
        {
            Debug.LogError($"File at path {path} is empty.");
            return default;
        }

        return ObjectFromXmlContent<T>(xmlContent);
    }

    /// <summary>
    /// Deserialize từ XML string thành object type T.
    /// </summary>
    public static T ObjectFromXmlContent<T>(string xmlContent)
    {
        XmlDocument doc = new XmlDocument();
        doc.XmlResolver = null;  // Fix #1: Disable external entity resolution (XXE protection)
        doc.LoadXml(xmlContent);
        XmlNode root = doc.DocumentElement;
        return DeserializeFromXml<T>(root, XmlConverterSettings.Default);
    }

    /// <summary>
    /// Deserialize XmlNode thành object type T.
    /// Đây là method chính, được gọi đệ quy cho nested objects.
    /// </summary>
    /// <param name="root">XmlNode cần deserialize</param>
    /// <param name="settings">Cấu hình (converters, processors, etc.)</param>
    /// <param name="isRecursive">true nếu đang gọi đệ quy (internal use)</param>
    public static T DeserializeFromXml<T>(XmlNode root, XmlConverterSettings settings = null, bool isRecursive = false)
    {
        if (settings == null)
        {
            settings = XmlConverterSettings.Default;
        }

        T obj = default(T);
        System.Type type = typeof(T);

        bool hasChildNodes = root.HasChildNodes;
        int childNodeCount = root.ChildNodes.Count;

        // Fix #2: Process attributes và lưu state (thay vì gọi Processor + CheckCondition riêng)
        var attributeState = ProcessAttributesAndGetState(root, settings);

        // Fix #2 + #6: Kiểm tra condition từ state (không đọc attribute trực tiếp)
        if (!CheckConditionFromState(attributeState))
        {
            return obj;
        }

        // Xử lý primitive type: <MyField>value</MyField>
        if (hasChildNodes && childNodeCount == 1 && root.FirstChild.NodeType == XmlNodeType.Text)
        {
            if (type == typeof(string))
            {
                return (T)((object)root.FirstChild.Value);
            }

            object convertedValue = GetObjectFromString<T>(root.FirstChild.Value, settings, root);
            return (T)convertedValue;
        }

        // Xử lý CDATA: <![CDATA[content]]>
        if (hasChildNodes && childNodeCount == 1 && root.FirstChild.NodeType == XmlNodeType.CDATA)
        {
            if (type != typeof(string))
            {
                Debug.LogError($"Cannot assign CDATA value to type {type.FullName}. Expected string.");
                return default;
            }
            return (T)((object)root.FirstChild.Value);
        }

        // Xác định type thực tế từ Class attribute (nếu có)
        bool isClassOrStruct = TypeUtils.IsClass(type) || TypeUtils.IsStruct(type);
        Type resolvedType = isClassOrStruct ? GetTypeClassFromXmlNode(root, settings, type) : type;
        Type underlyingType = Nullable.GetUnderlyingType(resolvedType) ?? resolvedType;

        if (!isClassOrStruct)
        {
            return obj;
        }

        // Xử lý collection (List<T>)
        if (TypeUtils.IsCollection(underlyingType))
        {
            if (TypeUtils.IsList(underlyingType))
            {
                return DeserializeList<T>(root, underlyingType, settings);
            }
            else
            {
                Debug.LogError($"Collection type {underlyingType.FullName} is not supported. Only List<T> is supported.");
                return default;
            }
        }

        // Fix #3: Tạo instance với exception handling đúng
        if (!typeof(T).IsAssignableFrom(underlyingType))
        {
            Debug.LogError(
                $"Type {underlyingType.FullName} is not assignable to {typeof(T).FullName}. " +
                $"Cannot create instance.");
            return default;
        }

        try
        {
            obj = (T)Activator.CreateInstance(underlyingType);
        }
        catch (MissingMethodException)
        {
            Debug.LogError(
                $"Type {underlyingType.FullName} has no parameterless constructor.");
            return default;
        }
        catch (TargetInvocationException ex)
        {
            Debug.LogError(
                $"Constructor of {underlyingType.FullName} threw exception: " +
                $"{ex.InnerException?.Message ?? ex.Message}");
            return default;
        }
        catch (MethodAccessException)
        {
            Debug.LogError(
                $"No access to constructor of {underlyingType.FullName}.");
            return default;
        }

        // Deserialize các child nodes thành fields
        if (hasChildNodes)
        {
            DeserializeFields(obj, root, underlyingType, settings);
        }

        // Post-process: gọi PostProcessAll SAU KHI object tạo xong
        if (attributeState.Count > 0)
        {
            settings.AttributeProcessorRegistry.PostProcessAll(root, settings, attributeState, obj);
        }

        return obj;
    }

    /// <summary>
    /// Deserialize List<T> từ XML node.
    /// </summary>
    private static T DeserializeList<T>(XmlNode root, Type listType, XmlConverterSettings settings)
    {
        Type elementType = listType.GetGenericArguments()[0];

        // Fix #4: Tạo instance từ CHÍNH listType (hỗ trợ subclass như MyIntList : List<int>)
        var listInstance = Activator.CreateInstance(listType);

        // Fix #5: Cache Add method theo elementType
        if (!_listAddMethodCache.TryGetValue(elementType, out var addMethod))
        {
            addMethod = listType.GetMethod("Add");
            _listAddMethodCache[elementType] = addMethod;
        }

        foreach (XmlNode child in root.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (child.Name != "li")
            {
                Debug.LogWarning(
                    $"Unexpected XML node name '{child.Name}' in collection. " +
                    $"Expected '<li>'. Skipping node.");
                continue;
            }

            // Fix #5: Dùng cached generic method
            var genericMethod = GetCachedDeserializeMethod(elementType);
            object elementValue = genericMethod.Invoke(null, new object[] { child, settings, true });
            addMethod.Invoke(listInstance, new object[] { elementValue });
        }

        return (T)listInstance;
    }

    /// <summary>
    /// Deserialize các child nodes thành fields của object.
    /// </summary>
    private static void DeserializeFields(object obj, XmlNode root, Type type, XmlConverterSettings settings)
    {
        foreach (XmlNode child in root.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            string fieldName = child.Name;

            // Tìm field (hỗ trợ case-insensitive nếu được bật)
            var fieldInfo = FindField(type, fieldName, settings);
            if (fieldInfo == null)
            {
                if (settings.IgnoreExtraFields)
                {
                    Debug.LogWarning(
                        $"Field '{fieldName}' not found in type {type.FullName}. Ignoring extra field.");
                }
                else
                {
                    Debug.LogError(
                        $"Field '{fieldName}' not found in type {type.FullName}.");
                }
                continue;
            }

            Type fieldType = fieldInfo.FieldType;

            // Fix #5: Dùng cached generic method
            var genericMethod = GetCachedDeserializeMethod(fieldType);
            object fieldValue = genericMethod.Invoke(null, new object[] { child, settings, true });
            fieldInfo.SetValue(obj, fieldValue);
        }
    }

    /// <summary>
    /// Lấy cached generic method cho DeserializeFromXml.
    /// </summary>
    private static MethodInfo GetCachedDeserializeMethod(Type elementType)
    {
        if (!_genericDeserializeCache.TryGetValue(elementType, out var genericMethod))
        {
            genericMethod = _deserializeMethod.MakeGenericMethod(elementType);
            _genericDeserializeCache[elementType] = genericMethod;
        }
        return genericMethod;
    }

    /// <summary>
    /// Tìm field theo tên, hỗ trợ case-insensitive.
    /// </summary>
    private static System.Reflection.FieldInfo FindField(Type type, string fieldName, XmlConverterSettings settings)
    {
        // Thử exact match trước
        var fieldInfo = type.GetField(
            fieldName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (fieldInfo != null)
        {
            return fieldInfo;
        }

        // Nếu bật case-insensitive, thử so sánh không phân biệt hoa/thường
        if (settings.CaseInsensitiveFieldMatching)
        {
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (string.Equals(field.Name, fieldName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
            }
        }

        return null;
    }
    
    /// <summary>
    /// Lấy type thực tế từ Class attribute trong XML node.
    ///
    /// Lưu ý: Class attribute có special handling built-in, KHÔNG dùng ClassAttributeProcessor.
    /// Lý do: Type resolution cần xảy ra TRƯỚC khi tạo instance, nên phải xử lý trực tiếp.
    /// </summary>
    private static Type GetTypeClassFromXmlNode(XmlNode root, XmlConverterSettings settings, Type defaultType = null)
    {
        // Đọc trực tiếp Class attribute (special handling, không qua processor)
        XmlAttribute xmlAttribute = root.Attributes["Class"];
        if (xmlAttribute != null)
        {
            string typeName = xmlAttribute.Value;
            Type foundType = TypeUtils.TryGetType(typeName);
            if (foundType != null)
            {
                return foundType;
            }
            else
            {
                Debug.LogError(
                    $"Type specified in XML not found: {typeName}. " +
                    $"Falling back to expected type: {defaultType?.FullName ?? "null"}");
                return defaultType;
            }
        }

        return defaultType;
    }

    /// <summary>
    /// Fix #2 + #6: Kiểm tra condition từ attributeState (không đọc attribute trực tiếp).
    /// </summary>
    private static bool CheckConditionFromState(Dictionary<string, object> attributeState)
    {
        if (!attributeState.TryGetValue(StateKeyConditionResult, out object result))
        {
            return true; // Không có condition = luôn true
        }

        if (result is bool shouldProcess)
        {
            return shouldProcess;
        }

        // Fallback: coi như true
        return true;
    }

    // Fix #2: Xử lý attributes và lưu state để CheckCondition dùng sau
    private static Dictionary<string, object> ProcessAttributesAndGetState(XmlNode root, XmlConverterSettings settings)
    {
        var state = new Dictionary<string, object>();
        if (root.Attributes != null && root.Attributes.Count > 0)
        {
            settings.AttributeProcessorRegistry.ProcessAttributes(root, settings, state);
        }
        return state;
    }

    /// <summary>
    /// Convert string value thành object type T using registered converters.
    /// </summary>
    /// <summary>
    /// Deserialize enum từ string. Hỗ trợ cả Flags và thường.
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    /// <param name="value">String value: "ValueA" hoặc "ValueA|ValueB" (flags)</param>
    public static T DeserializeEnum<T>(string value) where T : struct, System.Enum
    {
        Type enumType = typeof(T);

        // Thử parse trực tiếp (hỗ trợ cả tên và số)
        if (Enum.TryParse<T>(value, true, out T result))
            return result;

        // Flags enum: parse "ValueA|ValueB"
        if (enumType.IsDefined(typeof(FlagsAttribute), false))
        {
            string[] parts = value.Split('|');
            long combined = 0;
            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (Enum.TryParse<T>(trimmed, true, out T flag))
                    combined |= Convert.ToInt64(flag);
                else
                    Debug.LogWarning($"Unknown enum flag '{trimmed}' for type {enumType.Name}");
            }
            return (T)Enum.ToObject(enumType, combined);
        }

        Debug.LogError($"Cannot parse enum value '{value}' for type {enumType.Name}");
        return default;
    }

    /// <summary>
    /// Deserialize enum không generic (qua reflection).
    /// Dùng cho trường hợp T không có constraint struct, Enum.
    /// </summary>
    private static object DeserializeEnumReflect(Type enumType, string value)
    {
        // Thử parse trực tiếp (hỗ trợ cả tên và số)
        if (Enum.TryParse(enumType, value, true, out object result))
            return result;

        // Flags enum: parse "ValueA|ValueB"
        if (enumType.IsDefined(typeof(FlagsAttribute), false))
        {
            string[] parts = value.Split('|');
            long combined = 0;
            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (Enum.TryParse(enumType, trimmed, true, out object flag))
                    combined |= Convert.ToInt64(flag);
                else
                    Debug.LogWarning($"Unknown enum flag '{trimmed}' for type {enumType.Name}");
            }
            return Enum.ToObject(enumType, combined);
        }

        Debug.LogError($"Cannot parse enum value '{value}' for type {enumType.Name}");
        return Activator.CreateInstance(enumType);
    }

    private static object GetObjectFromString<T>(string value, XmlConverterSettings settings, XmlNode contextNode = null)
    {
        // Fallback: nếu type là enum, dùng DeserializeEnum qua reflection
        if (typeof(T).IsEnum)
        {
            return DeserializeEnumReflect(typeof(T), value);
        }

        if (settings.TryGetConverter(typeof(T), out XmlConverter converter))
        {
            if (!converter.canRead)
            {
                return value;
            }

            object result = converter.fromXML(value);
            if (result == null)
            {
                return default(T);
            }

            if (result is T typedResult)
            {
                return typedResult;
            }

            throw new InvalidCastException(
                $"Converter for type {typeof(T).FullName} returned incompatible value of type {result.GetType().FullName}.");
        }
        else
        {
            if (contextNode != null)
            {
                Debug.LogError(
                    $"No converter registered for type {typeof(T).FullName} " +
                    $"at XML node '{contextNode.Name}'. Value: '{value}'");
            }
            else
            {
                Debug.LogError(
                    $"No converter registered for type {typeof(T).FullName}. " +
                    $"Value string parse error: '{value}'");
            }
            return default(T);
        }
    }
}
