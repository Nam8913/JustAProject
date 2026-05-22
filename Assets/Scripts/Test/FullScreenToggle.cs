using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenToggle : TestToggle
{
    [SerializeField] private TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        myToggle.isOn = Screen.fullScreen;
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public override void OnToggleChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        Debug.Log("Full Screen is: " + isOn + " Screen.fullScreen: " + Screen.fullScreen);
    }

    private void OnGUI()
    {
        tmp.text = string.Concat(Screen.fullScreen ? "Full Screen" : "Windowed", '\n', myToggle.isOn ? "On" : "Off");
    }
}
