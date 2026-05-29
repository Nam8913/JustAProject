using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DevToolWindow : MonoBehaviour
{
    public static DevToolWindow Ins;
    public GameObject content_listOptionButton;
    public List<GameObject> pages;

    private Dictionary<string, GameObject> pageDict = new Dictionary<string, GameObject>();
    private string currentActivePageName;
    void Awake()
    {
        if(Ins != null)
        {
            Debug.LogError("Multiple instances of DevToolWindow detected. There should only be one instance of DevToolWindow in the scene.");
            Destroy(this);
            return;
        }
        Ins = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pages.ForEach(page => page.SetActive(false));
        pageDict = pages.ToDictionary(page => page.gameObject.name, page => page);
    }

    void Update()
    {
        if(InputSystem.GetDevice<Keyboard>().escapeKey.wasPressedThisFrame)
        {
            Debug.Log("Escape key pressed. Closing DevToolWindow.");
            gameObject.SetActive(false);
        }
    }

    public void SetActivePageByName(string pageName)
    {
        if(currentActivePageName == pageName)
        {
            return;
        }
        if(!string.IsNullOrEmpty(currentActivePageName))
        {
            pageDict[currentActivePageName].SetActive(false);
        }

        if(pageDict.TryGetValue(pageName, out GameObject page))
        {
            Debug.Log($"Activating page: {pageName}");
            currentActivePageName = page.gameObject.name;
            page.SetActive(true);
        }else
        {
            Debug.LogError($"Page with name {pageName} not found in DevToolWindow.");
        }
    }
}
