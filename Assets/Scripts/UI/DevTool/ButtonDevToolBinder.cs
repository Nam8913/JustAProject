using UnityEngine;
using UnityEngine.UI;

public class ButtonDevToolBinder : MonoBehaviour
{
    [SerializeField]
    private string key;
    private Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        if(button == null)        {
            Debug.LogError("ButtonDevToolBinder: No Button component found on this GameObject.");
            return;
        }
        button.onClick.AddListener(OnButtonClicked);

        TMPro.TMP_Text label = GetComponentInChildren<TMPro.TMP_Text>();
        if(label != null)
        {
            label.text = key;
        }
    }
    void OnButtonClicked()
    {
        if(string.IsNullOrEmpty(key))
        {
            Debug.LogError("ButtonDevToolBinder: Key is not set. Please set the key for this button.");
            return;
        }
        Debug.Log($"ButtonDevToolBinder: Button with key {key} clicked.");
        DevToolWindow.Ins.SetActivePageByName(key);
    }
}
