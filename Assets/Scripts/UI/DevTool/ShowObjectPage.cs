using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShowObjectPage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject sectionPrefab;
    [SerializeField]
    private GameObject content_sectionList;
    [SerializeField]
    private TMPro.TextMeshProUGUI informationText;
    [SerializeField]
    private Image previewImage;

    [Header("Data")]
    [SerializeField]
    static ShowObjectPage ins;
    private List<string> sections = new List<string>();
    private string currentSelectedObjectId = string.Empty;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ins != null)
        {
            Debug.LogError("Multiple instances of ShowObjectPage detected. There should only be one instance of ShowObjectPage in the scene.");
            Destroy(this);
            return;
        }
        ins = this;
        sections = DatabaseThing.Store.Keys.ToList().ConvertAll(type => type.Name);
        foreach(string section in sections)
        {
            GameObject sectionObj = Instantiate(sectionPrefab, content_sectionList.transform);
            ShowObjectSection showObjectSection = sectionObj.GetComponent<ShowObjectSection>();
            showObjectSection.SetSectionName(section);
            Type getType = TypeUtils.TryGetType(section);
            if(getType != null)
            {
                Dictionary<string, object> data = DatabaseThing.Store[getType];
                showObjectSection.SetList(data.Keys.ToList());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SelectedObjectToShowById(string section,string id)
    {
        if(ins.currentSelectedObjectId == id)
        {
            return;
        }
        ins.currentSelectedObjectId = id;
        ins.informationText.text = ins.GetInformationTextById(section, id);
    }

    string GetInformationTextById(string section, string id)
    {
        Type getType = TypeUtils.TryGetType(section);
        if(getType != null)
        {
            if(DatabaseThing.Store.TryGetValue(getType, out var dataDict))
            {
                if(dataDict.TryGetValue(id, out var data))
                {
                    if(TypeUtils.IsRawData(getType))
                    {
                        return ins.GetInforForRawDataType(getType, data);
                    }
                    return data.ToString();
                }
            }
        }
        return $"No data found for ID: {id}";
    }

    string GetInforForRawDataType(Type type, object data)
    {
        var lines = new List<string>();
        var visited = new HashSet<object>(); // prevent infinite cycles
        AppendFields(type, data, "", lines, visited, 0);
        return string.Join("\n", lines);
    }

    void AppendFields(Type type, object data, string indent, List<string> lines,
        HashSet<object> visited, int depth)
    {
        const int maxDepth = 3;
        if (depth > maxDepth)
        {
            lines.Add($"{indent}... (max depth reached)");
            return;
        }

        var converters = XmlConverterSettings.Default.converters;

        foreach (var field in type.GetFields())
        {
            object value = field.GetValue(data);
            string prefix = $"{indent}{field.Name}";

            if (value == null)
            {
                lines.Add($"{prefix}: null");
            }
            else if (value is System.Collections.IList listVal)
            {
                lines.Add($"{prefix}:");
                for (int i = 0; i < listVal.Count; i++)
                {
                    object item = listVal[i];
                    if (item == null)
                    {
                        lines.Add($"{indent}  [{i}] null");
                    }
                    else if (converters.TryGetValue(item.GetType(), out var conv))
                    {
                        lines.Add($"{indent}  [{i}] {conv.toXML(item)}");
                    }
                    else if (item.GetType().IsPrimitive || item is string)
                    {
                        lines.Add($"{indent}  [{i}] {item}");
                    }
                    else if (item.GetType().IsEnum)
                    {
                        lines.Add($"{indent}  [{i}] {item}");
                    }
                    else if (item.GetType().IsClass || item.GetType().IsValueType)
                    {
                        lines.Add($"{indent}  [{i}] {item.GetType().Name}");
                        if (visited.Add(item))
                            AppendFields(item.GetType(), item, indent + "    ", lines, visited, depth + 1);
                    }
                    else
                    {
                        lines.Add($"{indent}  [{i}] {item}");
                    }
                }
            }
            else if (value is System.Collections.IDictionary dictVal)
            {
                lines.Add($"{prefix}:");
                int idx = 0;
                foreach (var k in dictVal.Keys)
                {
                    object v = dictVal[k];
                    lines.Add($"{indent}  [{idx}]");
                    AppendFieldValue(indent + "    ", "key", k, lines, visited, depth);
                    AppendFieldValue(indent + "    ", "val", v, lines, visited, depth);
                    idx++;
                }
            }
            else if (converters.TryGetValue(field.FieldType, out var converter))
            {
                lines.Add($"{prefix}: {converter.toXML(value)}");
            }
            else if (field.FieldType.IsEnum)
            {
                lines.Add($"{prefix}: {value}");
            }
            else if (field.FieldType.IsClass || field.FieldType.IsValueType)
            {
                if (visited.Add(value))
                {
                    lines.Add($"{prefix}: {value.GetType().Name}");
                    AppendFields(field.FieldType, value, indent + "  ", lines, visited, depth + 1);
                }
                else
                {
                    lines.Add($"{prefix}: (circular reference)");
                }
            }
            else
            {
                lines.Add($"{prefix}: {value}");
            }
        }
    }

    void AppendFieldValue(string indent, string label, object value,
        List<string> lines, HashSet<object> visited, int depth)
    {
        var converters = XmlConverterSettings.Default.converters;
        if (value == null)
        {
            lines.Add($"{indent}{label}: null");
        }
        else if (converters.TryGetValue(value.GetType(), out var conv))
        {
            lines.Add($"{indent}{label}: {conv.toXML(value)}");
        }
        else if (value.GetType().IsEnum)
        {
            lines.Add($"{indent}{label}: {value}");
        }
        else if (value.GetType().IsClass || value.GetType().IsValueType)
        {
            if (visited.Add(value))
            {
                lines.Add($"{indent}{label}: {value.GetType().Name}");
                AppendFields(value.GetType(), value, indent + "  ", lines, visited, depth + 1);
            }
            else
            {
                lines.Add($"{indent}{label}: (circular reference)");
            }
        }
        else
        {
            lines.Add($"{indent}{label}: {value}");
        }
    }
}
