using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleBinder : MonoBehaviour
{
    [SerializeField]
    private string key;
    
    [SerializeField]
    private SettingsWindow settingsWindow;

    private Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null)
        {
            Debug.LogError("ToggleBinder requires a Toggle component on the same GameObject.");
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggle.isOn = settingsWindow.GetBoolValueForToggle(key);
    }

    void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value)
    {
        settingsWindow.SetBoolValueForToggle(key, value);
        toggle.isOn = settingsWindow.GetBoolValueForToggle(key);
    }
}

public static class ToggleBinderDict
{
    private static Dictionary<string, (Func<bool>,Action<bool>)> toggleStateResolvers = new Dictionary<string, (Func<bool>,Action<bool>)>()
    {
        {
            "devmode",
            (
                () => {
                    bool value = GameService.Settings.IsDevMode();
                    Debug.Log("Getting dev mode value: " + value);
                    return value; 
                },
                (value) => GameService.Settings.SetDevMode(value)
            )
        },
        {
            "fullscreen",
            (
                () => {
                    bool value = GameService.Settings.IsFullScreen();
                    Debug.Log("Getting fullscreen value:" + value);
                    return value; 
                },
                (value) => GameService.Settings.SetFullScreen(value)
            )
        },
        {
            "runinbackground",
            (
                () => {
                    bool value = GameService.Settings.IsRunInBackground();
                    Debug.Log("Getting run in background value: " + value);
                    return value; 
                },
                (value) => GameService.Settings.SetRunInBackgroundable(value)
            )
        }
    };

    public static Dictionary<string, (Func<bool>,Action<bool>)> Dict
    {
        get
        {
            return toggleStateResolvers;
        }
    }
}
