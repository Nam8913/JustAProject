using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowObjectSection : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject toggleHeader;
    [SerializeField]
    private GameObject listButton;
    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;
    [SerializeField]
    private GameObject buttonPrefab;// itemUI_button prefab

    [Header("Temp Data")]
    private string section;
    private float height;
    [SerializeField]
    private List<string> list = new List<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridLayoutGroup = listButton.GetComponent<GridLayoutGroup>();
        MakeButtons();
        
    }

    void MakeButtons()
    {
        float width = listButton.GetComponent<RectTransform>().rect.width;
        int maxColumns = Mathf.FloorToInt(width / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x));
        
        height = 0;
        float toggleH = toggleHeader.GetComponent<RectTransform>().rect.height;
        height += toggleH;
        int count = 0;
        int currRow = 0;
        foreach (var item in list)
        {
            GameObject button = InstantiateButton(item);
            count++;
            int rw =  Mathf.CeilToInt((float)count / maxColumns);
            if (rw > currRow)
            {
                currRow = rw;
                float buttonH = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
                height += buttonH; // Add spacing for new row
                
                Debug.Log($"new Height: {buttonH}");
            }
        }
        Resize_Height(height);
    }

    void Resize_Height(float height)
    {
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    GameObject InstantiateButton(string id)
    {
        GameObject button = Instantiate(buttonPrefab, listButton.transform);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = id;
        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnButtonClick(section, id));

        return button;
    }

    public void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"Toggle value changed: {isOn}");
        if(isOn)
        {
            listButton.SetActive(true);
            Resize_Height(height);
        }
        else
        {
            listButton.SetActive(false);
            Resize_Height(toggleHeader.GetComponent<RectTransform>().rect.height);
        }
    }

    private void OnButtonClick(string section, string value)
    {
        Debug.Log($"Button clicked: {value} section: {section}");
        ShowObjectPage.SelectedObjectToShowById(section, value);
    }

    public void SetSectionName(string section)
    {
        this.section = section;
        toggleHeader.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = section;
    }

    public void SetList(List<string> list)
    {
        this.list = list;
    }
}
