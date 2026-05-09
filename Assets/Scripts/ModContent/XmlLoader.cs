using System;
using System.IO;
using System.Xml;
using UnityEngine;
using ModContent.XmlConverter;
using System.Collections.Generic;

public static class XmlLoader
{
    public static T LoadFromXml<T>(string path)
    {
        if(!File.Exists(path))
        {
            Debug.LogError($"File not found at path: {path}");
            return default;
        }

        string xmlContent = File.ReadAllText(path);
        if(string.IsNullOrEmpty(xmlContent))
        {
            Debug.LogError($"File at path {path} is empty.");
            return default;
        }

        return ObjectFromXmlContent<T>(xmlContent);
    }

    public static T ObjectFromXmlContent<T>(string xmlContent)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlContent);
        XmlNode root = doc.DocumentElement;
        return DeserializeFromXml<T>(root, XmlConverterSettings.Default);
    }

    public static T DeserializeFromXml<T>(XmlNode root ,XmlConverterSettings settings = null,bool isRecursive = false)
    {
        if(settings == null)
        {
            settings = XmlConverterSettings.Default;
        }
        T obj = default(T);
        System.Type type = typeof(T);

        bool hasChildNodes = root.HasChildNodes;
        bool hasAttributes = root.Attributes != null && root.Attributes.Count > 0;
        int childNodeCount = root.ChildNodes.Count;

        if(hasAttributes)
        {
            settings.HandlerAttribute(root);
        }

        // If the node has exactly one child and that child is a text node, we can directly assign the value to the object if it's a simple type (like string, int, etc.)
        if(hasChildNodes && childNodeCount == 1 && root.FirstChild.NodeType == XmlNodeType.Text)
        {
            if(type == typeof(string))
            {
                return (T)((object)root.FirstChild.Value);
            }
            //object convertedValue = Convert.ChangeType(root.FirstChild.Value, type);
            object convertedValue = GetObjectFromString<T>(root.FirstChild.Value, settings);
            return (T)convertedValue;
        }

        if(hasChildNodes && childNodeCount == 1 && root.FirstChild.NodeType == XmlNodeType.CDATA)
        {
            
            if(type != typeof(string))
            {
                Debug.LogError($"Cannot assign CDATA value to type {type.FullName}. Expected string.");
                return default;
            }
            return (T)((object)root.FirstChild.Value);
        }

        bool isClassOrStruct = TypeUtils.IsClass(type) || TypeUtils.IsStruct(type);
        Type type2 = isClassOrStruct ? GetTypeClassFromXmlNode(root,type) : type;
        Type type3 = Nullable.GetUnderlyingType(type2) ?? type2;
        
        if(!isClassOrStruct)
        {
            return obj;
        }

        if(TypeUtils.IsCollection(type3))
        {
            if(TypeUtils.IsList(type3))
            {
                Type elementType = type3.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var listInstance = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");

                foreach(XmlNode child in root.ChildNodes)
                {
                    if(child.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if(child.Name != "li")
                    {
                        Debug.LogWarning($"Unexpected XML node name '{child.Name}' in collection. Expected '<li>'. Skipping node.");
                        continue;
                    }

                    var method = typeof(XmlLoader).GetMethod("DeserializeFromXml", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var genericMethod = method.MakeGenericMethod(elementType);
                    object elementValue = genericMethod.Invoke(null, new object[] { child, settings, true });
                    addMethod.Invoke(listInstance, new object[] { elementValue });
                }

                return (T)listInstance;
            }
            else
            {
                Debug.LogError($"Collection type {type3.FullName} is not supported. Only List<T> is supported.");
                return default;
            }
        }
        
        try
        {
            obj = (T)Activator.CreateInstance(type3);
        }catch(InvalidCastException ex)
        {
            throw new InvalidCastException($"Failed to create instance of type {type3.FullName}. Ensure it has a parameterless constructor. Original error: {ex.Message} \n {ex.StackTrace}", ex);
        }
        if(hasChildNodes)
        {
            foreach(XmlNode child in root.ChildNodes)
            {
                if(child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                string fieldName = child.Name;
                var fieldInfo = type3.GetField(fieldName);
                if(fieldInfo == null)
                {
                    if(settings.IgnoreExtraFields)
                    {
                        Debug.LogWarning($"Field {fieldName} not found in type {type3.FullName}. Ignoring extra field.");
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"Field {fieldName} not found in type {type3.FullName}.");
                        continue;
                    }
                }

                Type fieldType = fieldInfo.FieldType;
                var method = typeof(XmlLoader).GetMethod("DeserializeFromXml", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var genericMethod = method.MakeGenericMethod(fieldType);
                object fieldValue = genericMethod.Invoke(null, new object[] { child, settings, true });
                fieldInfo.SetValue(obj, fieldValue);
            }
        }
            
        
        return obj;
    }

    private static Type GetTypeClassFromXmlNode(XmlNode root, Type defaultType = null)
    {
        XmlAttribute xmlAttribute = root.Attributes["Class"];
        if(xmlAttribute != null)
        {
            string typeName = xmlAttribute.Value;
            Type foundType = TypeUtils.TryGetType(typeName);
            if(foundType != null)
            {
                return foundType;
            }
            else
            {
                Debug.LogError($"Type specified in XML not found: {typeName}. Falling back to expected type: {defaultType.FullName}");
                return defaultType;
            }
        }
        
        return defaultType;
    }

    private static object GetObjectFromString<T>(String value, XmlConverterSettings settings)
    {
        if(settings.TryGetConverter(typeof(T), out XmlConverter converter))
        {
            if(!converter.canRead)
            {
                return value;
            }

            object result = converter.fromXML(value);
            if(result == null)
            {
                return default(T);
            }

            if(result is T typedResult)
            {
                return typedResult;
            }

            throw new InvalidCastException($"Converter for type {typeof(T).FullName} returned incompatible value of type {result.GetType().FullName}.");
        }
        else
        {
            Debug.LogError($"No converter registered for type {typeof(T).FullName}");
            return default(T);
        }
    }
}
