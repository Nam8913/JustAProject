using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    [Header("References_UI")]
    [SerializeField]
    private GameObject content_SettingPages;
    [SerializeField]
    private GameObject settingPageButtonPrefab;
    [SerializeField]
    private GameObject storagePage;

    [SerializeField]
    private List<GameObject> settingsPages;
    private int currentPageIndex = 0;
    
    #region Dev
    private List<string> devPageNames = new List<string> { "General", "Graphics", "Audio", "Controls", "Gameplay", "Mods"};
    #endregion

    private void OnEnable()
    {
        CreateSettingPageButtons();
    }

    private void OnDisable()
    {
        ClearSettingPageButtons();
    }

    private void ClearSettingPageButtons()
    {
        // This method will clear all the buttons from the content_SettingPages
        foreach(Transform child in content_SettingPages.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateSettingPageButtons()
    {
        // This method will be responsible for creating buttons for each settings page and adding them to the content_SettingPages
        // For now, we can just create buttons for the devPageNames for testing purposes
        foreach(string page in devPageNames)
        {
            GameObject buttonObj = Instantiate(settingPageButtonPrefab, content_SettingPages.transform);
            Button button = buttonObj.GetComponent<Button>();
            TMPro.TMP_Text buttonText = buttonObj.GetComponentInChildren<TMPro.TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = page; 
            }
            int index = devPageNames.IndexOf(page);
            button.onClick.AddListener(() => OnSettingPageButtonClicked(index));

            //get anchor and pivot of the button
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(160, 40); // Set the height of the button to 30, width will be determined by the layout group
        }
    }
    
    private void OnSettingPageButtonClicked(int index)
    {
        GameObject lastPage = storagePage.transform.Find(devPageNames[currentPageIndex])?.gameObject;
        if (lastPage != null)
        {
            lastPage.SetActive(false);
        }

        currentPageIndex = index;
        string pageName = devPageNames[index];
        GameObject pageToShow = storagePage.transform.Find(pageName)?.gameObject;
        if(pageToShow != null)
        {
            pageToShow.SetActive(true);
        }
        else
        {
            Debug.LogError($"Settings page '{pageName}' not found in storagePage.");
        }

        //DrawContentPage(index, pageToShow);
    }

    private void DrawContentPage(int index,GameObject page)
    {
    }

    public bool GetBoolValueForToggle(string key, bool debugIfNotFound = true)
    {
        if(ToggleBinderDict.Dict.ContainsKey(key))
        {
            return ToggleBinderDict.Dict[key].Item1();
        }
        if(debugIfNotFound)
        {
            Debug.LogError($"Bool value for toggle '{key}' not found.");
        }
        return false;
    }

    public void SetBoolValueForToggle(string key, bool value)
    {
        if(ToggleBinderDict.Dict.ContainsKey(key))
        {
            ToggleBinderDict.Dict[key].Item2(value);
        }
    }

    
}
