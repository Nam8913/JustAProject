using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        string debug = GameService.Game.Settings.DebugString();
        Debug.Log("Game Started with settings: " + debug);
        TextMeshProUGUI tmp = GameObject.Find("Canvas/Panel/Text").GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = debug;
        }

        foreach(var res in GameService.Game.Settings.showAvailableResolutions())
        {
            Debug.Log($"Available Resolution: {res}");
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }

    public virtual void LateUpdate()
    {
        
    }

    public virtual void OnEnable()
    {
        
    }

    public virtual void OnDisable()
    {
        
    }

    public virtual void OnDestroy()
    {
        
    }

    
}
