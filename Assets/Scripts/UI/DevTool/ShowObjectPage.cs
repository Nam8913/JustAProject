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
        return string.Join("\n", type.GetFields().Select(field => $"{field.Name}: {field.GetValue(data)}"));
    }
}
