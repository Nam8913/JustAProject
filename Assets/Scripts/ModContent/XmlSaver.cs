using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ModContent.XmlConverter;
using UnityEngine;

public static class XmlSaver
{
	public static void SaveObjectToXml<T>(T value, string path, XmlConverterSettings settings = null, string rootName = null)
	{
		string xmlContent = ObjectToXmlContent(value, settings, rootName);
		WriteXmlToPath(path, xmlContent);
	}

	public static string ObjectToXmlContent<T>(T value, XmlConverterSettings settings = null, string rootName = null)
	{
		settings ??= XmlConverterSettings.Default;

		XmlDocument document = new XmlDocument();
		document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", null));

		Type declaredType = typeof(T);
		Type actualType = value != null ? value.GetType() : declaredType;
		string elementName = string.IsNullOrEmpty(rootName) ? GetXmlElementName(actualType) : rootName;

		XmlElement root = SerializeValue(document, value, declaredType, elementName, settings);
		document.AppendChild(root);

		return ToFormattedXml(document);
	}

	public static void SaveCollectionToXml<T>(IEnumerable<T> values, string path, string rootName = "Data", XmlConverterSettings settings = null)
	{
		string xmlContent = CollectionToXmlContent(values, rootName, settings);
		WriteXmlToPath(path, xmlContent);
	}

	public static string CollectionToXmlContent<T>(IEnumerable<T> values, string rootName = "Data", XmlConverterSettings settings = null)
	{
		settings ??= XmlConverterSettings.Default;

		XmlDocument document = new XmlDocument();
		document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", null));

		XmlElement root = document.CreateElement(string.IsNullOrEmpty(rootName) ? "Data" : rootName);
		document.AppendChild(root);

		if (values != null)
		{
			Type declaredType = typeof(T);
			foreach (T value in values)
			{
				if (value == null)
				{
					continue;
				}

				XmlElement item = SerializeValue(document, value, declaredType, GetXmlElementName(value.GetType()), settings);
				root.AppendChild(item);
			}
		}

		return ToFormattedXml(document);
	}

	private static XmlElement SerializeValue(XmlDocument document, object value, Type declaredType, string elementName, XmlConverterSettings settings)
	{
		XmlElement element = document.CreateElement(elementName);

		if (value == null)
		{
			return element;
		}

		Type actualType = value.GetType();

		if (TryGetConverter(settings, declaredType, value, out XmlConverter converter))
		{
			element.InnerText = converter.toXML(value) ?? string.Empty;
			return element;
		}

		if (TypeUtils.IsCollection(actualType))
		{
			if (!TypeUtils.IsList(actualType))
			{
				Debug.LogError($"Collection type {actualType.FullName} is not supported. Only List<T> is supported.");
				return element;
			}

			Type itemType = actualType.GetGenericArguments()[0];
			IEnumerable items = value as IEnumerable;
			if (items == null)
			{
				return element;
			}

			foreach (object item in items)
			{
				if (item == null)
				{
					continue;
				}

				XmlElement itemElement = SerializeValue(document, item, itemType, "li", settings);
				element.AppendChild(itemElement);
			}

			return element;
		}

		if (TypeUtils.IsClass(actualType) || TypeUtils.IsStruct(actualType))
		{
			if (declaredType != actualType)
			{
				element.SetAttribute("Class", GetTypeReferenceName(actualType));
			}

			foreach (FieldInfo fieldInfo in GetSerializableFields(actualType))
			{
				object fieldValue = fieldInfo.GetValue(value);
				Type fieldType = fieldInfo.FieldType;

				if (fieldValue == null && !TypeUtils.IsList(fieldType))
				{
					continue;
				}

				XmlElement child = SerializeValue(document, fieldValue, fieldType, fieldInfo.Name, settings);
				element.AppendChild(child);
			}

			return element;
		}

		element.InnerText = value.ToString() ?? string.Empty;
		return element;
	}

	private static bool TryGetConverter(XmlConverterSettings settings, Type declaredType, object value, out XmlConverter converter)
	{
		converter = null;

		if (settings == null)
		{
			return false;
		}

		if (declaredType != null && settings.TryGetConverter(declaredType, out converter) && converter.canWrite)
		{
			return true;
		}

		if (value is Type && settings.TryGetConverter(typeof(Type), out converter) && converter.canWrite)
		{
			return true;
		}

		Type runtimeType = value?.GetType();
		if (runtimeType != null && runtimeType != declaredType && settings.TryGetConverter(runtimeType, out converter) && converter.canWrite)
		{
			return true;
		}

		converter = null;
		return false;
	}

	private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
	{
		return type
			.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.OrderBy(field => field.MetadataToken).Select(field => field)
            .Where(field => !field.IsDefined(typeof(NonSerializedAttribute),false));
	}

	private static string GetXmlElementName(Type type)
	{
		return type?.Name ?? "Object";
	}

	private static string GetTypeReferenceName(Type type)
	{
		return type?.FullName ?? type?.Name ?? string.Empty;
	}

	private static string ToFormattedXml(XmlDocument document)
	{
		XmlWriterSettings writerSettings = new XmlWriterSettings
		{
			Indent = true,
			Encoding = new UTF8Encoding(false),
			OmitXmlDeclaration = false,
			NewLineOnAttributes = false
		};

		using MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter writer = XmlWriter.Create(memoryStream, writerSettings))
		{
			document.Save(writer);
		}

		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private static void WriteXmlToPath(string path, string xmlContent)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			throw new ArgumentException("XML path cannot be null or empty.", nameof(path));
		}

		string directoryPath = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}

		File.WriteAllText(path, xmlContent, new UTF8Encoding(false));
	}
}
