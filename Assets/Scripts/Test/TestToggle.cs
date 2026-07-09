#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;
using UnityEngine.UI;

public class TestToggle : MonoBehaviour
{
    public Toggle myToggle;
    public string textLabel;

    public virtual void Start()
    {
        //myToggle.onValueChanged.AddListener(OnToggleChanged);
        this.gameObject.transform.Find("Label").GetComponent<Text>().text = textLabel;
    }

    public virtual void OnToggleChanged(bool isOn)
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log("Toggle is: " + isOn);
        #endif
    }
}