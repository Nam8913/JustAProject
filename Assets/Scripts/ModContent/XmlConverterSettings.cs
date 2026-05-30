using System.Collections.Generic;
using System.Xml;
using ModContent.XmlConverter;
using ModContent.XmlExtendedAttribute;
using UnityEngine;
public class XmlConverterSettings
{
    public bool IgnoreMissingFields { get; set; } = true;
    public bool IgnoreExtraFields { get; set; } = true;
    public bool CaseInsensitiveFieldMatching { get; set; } = true;

   private IXmlExtendedAttribute context;

   public Dictionary<System.Type, XmlConverter> converters = new Dictionary<System.Type, XmlConverter>();

   public void HandlerAttribute(XmlNode node)
    {
        Dictionary<string, IXmlExtendedAttribute> extendedAttributes = new Dictionary<string, IXmlExtendedAttribute>();
        //extendedAttributes = 
        foreach(var extendedAttribute in DefaultExtendedAttributes)
        {
            if(extendedAttributes.ContainsKey(extendedAttribute.keyAttribute))
            {
                Debug.LogError($"An extended attribute with key {extendedAttribute.keyAttribute} is already registered. Skipping registration of extended attribute of type {extendedAttribute.GetType().Name}.");
                continue;
            }
            extendedAttributes[extendedAttribute.keyAttribute] = extendedAttribute;
        }
        
        if(node.Attributes != null)
        {
            foreach(XmlAttribute attr in node.Attributes)
            {
                extendedAttributes.TryGetValue(attr.Name, out var extendedAttribute);
                if(extendedAttribute != null)
                {
                    context = extendedAttribute;
                    if(context.condition(node))
                    {
                        context.SolveAtt(node, attr.Value, this);
                    }
                }
            }
        }
    }

    public void RegisterConverter<T>(XmlConverter converter , bool overrideExisting = false)
    {
        if(converters.ContainsKey(typeof(T)))
        {
            if(!overrideExisting)
            {
                throw new System.Exception($"A converter for type {typeof(T).FullName} is already registered.");
            }
        }
        converters[typeof(T)] = converter;
    }

    public bool TryGetConverter(System.Type type, out XmlConverter converter)
    {
        return converters.TryGetValue(type, out converter);
    }

    public static XmlConverterSettings Default => new XmlConverterSettings()
    {
            IgnoreMissingFields = true,
            IgnoreExtraFields = true,
            CaseInsensitiveFieldMatching = true,
            converters = new Dictionary<System.Type, XmlConverter>()
            {
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
                { typeof(System.Type), new TypeXmlConverter() }
            }
    };

    public static List<IXmlExtendedAttribute> DefaultExtendedAttributes => new List<IXmlExtendedAttribute>()
    {
        new GameObjectTarget()
    };
}