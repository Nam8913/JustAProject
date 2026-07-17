using System.Collections.Generic;
using System.Xml;
using ModContent.XmlConverter;
using ModContent.XmlExtendedAttribute;
using UnityEngine;

/// <summary>
/// Cấu hình cho quá trình deserialize XML.
/// Bao gồm: converters, attribute processors, và các tùy chọn khác.
/// </summary>
public class XmlConverterSettings
{
    /// <summary>
    /// Bỏ qua các field không tồn tại trong type khi deserialize.
    /// true: log warning và bỏ qua.
    /// false: log error và bỏ qua.
    /// </summary>
    public bool IgnoreMissingFields { get; set; } = true;

    /// <summary>
    /// Bỏ qua các field thừa trong XML (field không có trong type).
    /// true: log warning và bỏ qua.
    /// false: log error và bỏ qua.
    /// </summary>
    public bool IgnoreExtraFields { get; set; } = true;

    /// <summary>
    /// So sánh field name không phân biệt hoa/thường.
    /// true: "MyField" == "myfield" == "MYFIELD"
    /// false: "MyField" != "myfield"
    /// </summary>
    public bool CaseInsensitiveFieldMatching { get; set; } = true;

    /// <summary>
    /// Registry chứa tất cả attribute processors.
    /// Thay thế cho DefaultExtendedAttributes (deprecated).
    /// </summary>
    public XmlAttributeProcessorRegistry AttributeProcessorRegistry { get; set; }

    /// <summary>
    /// Dictionary chứa converters cho các primitive types.
    /// Key = Type, Value = XmlConverter.
    /// </summary>
    public Dictionary<System.Type, XmlConverter> converters = new Dictionary<System.Type, XmlConverter>();

    /// <summary>
    /// Constructor mặc định. Tạo registry với các processors mặc định.
    /// </summary>
    public XmlConverterSettings()
    {
        AttributeProcessorRegistry = new XmlAttributeProcessorRegistry();
        RegisterDefaultAttributeProcessors();
    }

    /// <summary>
    /// Xử lý tất cả attributes trong XmlNode.
    /// Delegate cho AttributeProcessorRegistry.
    /// 
    /// Deprecated: Gọi trực tiếp AttributeProcessorRegistry.ProcessAttributes() thay vì方法 này.
    /// </summary>
    [System.Obsolete("Use AttributeProcessorRegistry.ProcessAttributes() directly.")]
    public void HandlerAttribute(XmlNode node)
    {
        AttributeProcessorRegistry.ProcessAttributes(node, this);
    }

    /// <summary>
    /// Đăng ký converter cho 1 type cụ thể.
    /// </summary>
    /// <param name="converter">Converter instance</param>
    /// <param name="overrideExisting">true = ghi đè nếu đã tồn tại</param>
    public void RegisterConverter<T>(XmlConverter converter, bool overrideExisting = false)
    {
        if (converters.ContainsKey(typeof(T)))
        {
            if (!overrideExisting)
            {
                throw new System.Exception(
                    $"A converter for type {typeof(T).FullName} is already registered.");
            }
        }
        converters[typeof(T)] = converter;
    }

    /// <summary>
    /// Thử lấy converter theo type.
    /// </summary>
    public bool TryGetConverter(System.Type type, out XmlConverter converter)
    {
        return converters.TryGetValue(type, out converter);
    }

    /// <summary>
    /// Đăng ký 1 attribute processor mới.
    /// </summary>
    public void RegisterAttributeProcessor(XmlAttributeProcessor processor)
    {
        AttributeProcessorRegistry.Register(processor);
    }

    /// <summary>
    /// Đăng ký nhiều attribute processors cùng lúc.
    /// </summary>
    public void RegisterAttributeProcessors(IEnumerable<XmlAttributeProcessor> processors)
    {
        AttributeProcessorRegistry.RegisterAll(processors);
    }

    /// <summary>
    /// Lấy processor theo attribute name.
    /// </summary>
    public XmlAttributeProcessor GetAttributeProcessor(string attributeName)
    {
        return AttributeProcessorRegistry.GetProcessor(attributeName);
    }

    /// <summary>
    /// Xóa processor theo attribute name.
    /// </summary>
    public bool UnregisterAttributeProcessor(string attributeName)
    {
        return AttributeProcessorRegistry.Unregister(attributeName);
    }

    /// <summary>
    /// Tạo XmlConverterSettings mặc định với tất cả converters và processors có sẵn.
    /// </summary>
    public static XmlConverterSettings Default => new XmlConverterSettings()
    {
        IgnoreMissingFields = true,
        IgnoreExtraFields = true,
        CaseInsensitiveFieldMatching = true,
        converters = new Dictionary<System.Type, XmlConverter>()
        {
            // Primitive types
            { typeof(string), new StringXmlConverter() },
            { typeof(int), new IntXmlConverter() },
            { typeof(float), new FloatXmlConverter() },
            { typeof(bool), new BoolXmlConverter() },
            { typeof(double), new DoubleXmlConverter() },
            { typeof(long), new LongXmlConverter() },
            { typeof(short), new ShortXmlConverter() },
            { typeof(byte), new ByteXmlConverter() },
            { typeof(char), new CharXmlConverter() },
            { typeof(uint), new UInt32XmlConverter() },
            { typeof(ulong), new UInt64XmlConverter() },
            { typeof(ushort), new UInt16XmlConverter() },
            { typeof(sbyte), new SByteXmlConverter() },
            { typeof(decimal), new DecimalXmlConverter() },

            // Unity types
            { typeof(UnityEngine.Vector2), new UnityEngineVector2Converter() },
            { typeof(UnityEngine.Vector3), new UnityEngineVector3Converter() },
            { typeof(UnityEngine.Vector4), new UnityEngineVector4Converter() },
            { typeof(UnityEngine.Vector2Int), new UnityEngineVector2IntConverter() },
            { typeof(UnityEngine.Vector3Int), new UnityEngineVector3IntConverter() },

            // System types
            { typeof(System.Type), new TypeXmlConverter() },

            // Enum types (chỉ dùng cho serialize, deserialize dùng DeserializeEnum<T>)
            { typeof(System.Enum), new EnumXmlConverter() }
        }
    };

    /// <summary>
    /// Đăng ký tất cả processors mặc định vào registry.
    ///
    /// Lưu ý: Class attribute KHÔNG có processor riêng vì nó có special handling
    /// built-in trong XmlLoader.GetTypeClassFromXmlNode(). Lý do:
    /// - Type resolution cần xảy ra TRƯỚC khi tạo instance
    /// - Nếu dùng processor, state sẽ bị mất giữa attribute processing và deserialization
    /// </summary>
    private void RegisterDefaultAttributeProcessors()
    {
        // Core processors
        AttributeProcessorRegistry.Register(new GameObjectTargetAttributeProcessor());
        AttributeProcessorRegistry.Register(new ConditionalAttributeProcessor());
    }
}
