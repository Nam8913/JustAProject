#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DropdownBinder : MonoBehaviour
{
    [SerializeField]
    private string key;
    private TMPro.TMP_Dropdown dropdown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dropdown = GetComponent<TMPro.TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogError("DropdownBinder requires a Dropdown component on the same GameObject.");
            return;
        }
        if (DropdownBinderDict.dropdownKeyToActionMap.TryGetValue(key, out var dropdownData))
        {
            dropdown.ClearOptions();
            List<string> options = dropdownData.Item1();
            options.ForEach(option => 
            {
                #if DEBUG_LOG_FLAG && false
                Debug.Log("Adding dropdown option: " + option);
                #endif
            });
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }
        else
        {
            Debug.LogError("No dropdown data found for GameObject with name: " + gameObject.name);
        }
    }

    private void OnValueChanged(int value)
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log("Selected dropdown value: " + value);
        #endif
        // Here you would add the actual logic to handle the dropdown value change
        if (DropdownBinderDict.dropdownKeyToActionMap.TryGetValue(key, out var dropdownData))
        {
            dropdownData.Item2(value);
        }
    }
}

public static class DropdownBinderDict
{
    public static Dictionary<string, (Func<List<string>>, Action<int>)> dropdownKeyToActionMap = new Dictionary<string, (Func<List<string>>, Action<int>)>()
    {
        {
            "resolution", 
            (
                () => GameService.Settings.showAvailableResolutions().ToList(), 
                (int index) =>
                {
                    #if DEBUG_LOG_FLAG && false
                    Debug.Log("Selected resolution index: " + index);
                    #endif
                }
            )
        },
        {
            "fullscreenmode",
            (
                () => Enum.GetNames(typeof(FullScreenMode)).ToList(),
                (int index) =>
                {
                    switch (index)
                    {
                        case 0:
                            GameService.Settings.SetExclusiveFullscreen();
                            break;
                        case 1:
                            GameService.Settings.SetFullscreenWindow();
                            break;
                        case 2:
                            GameService.Settings.SetMaximized();
                            break;
                        case 3:
                            GameService.Settings.SetWindowed();
                            break;
                        default:
                            Debug.LogError("Invalid fullscreen mode index: " + index);
                            break;
                    }
                }
            )
        },
        {
            "selectModeGamePlaying",
            (
                () => new List<string> { "Survival", "Apocalypse", "Hardcore", "Sandbox", "Creative", "Story", "Custom" },
                (int index) =>
                {
                    #if DEBUG_LOG_FLAG && false
                    Debug.Log("Selected gameplay mode index: " + index);
                    #endif
                }
            )
        },
        {
            "selectFilterModeGamePlaying",
            (
                () => new List<string> { "All", "Survival", "Apocalypse", "Hardcore", "Sandbox", "Creative", "Story", "Custom" },
                (int index) =>
                {
                    #if DEBUG_LOG_FLAG && false
                    Debug.Log("Selected filter mode index: " + index);
                    #endif
                }
            )
        }
    };
}
