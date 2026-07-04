using System.Collections.Generic;
using UnityEngine;

public class HolderManager : MonoBehaviour
{
    private static HolderManager _instance;
    public static HolderManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject holderManagerObject = new GameObject("HolderManager");
                _instance = holderManagerObject.AddComponent<HolderManager>();
                DontDestroyOnLoad(holderManagerObject);
            }
            return _instance;
        }
    }
    public Dictionary<string, HolderObject> holderObjects;
    [SerializeField]
    [ReadOnly]
    private List<string> holders; // just for debugging purposes, to see the list of holder names in the inspector

    public static readonly string TrashHolderName = "TrashHolder";
    public static readonly string StructureHolderName = "StructureHolder";

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        holderObjects = new Dictionary<string, HolderObject>();

        // Create default trash/prefab holder objects when create GameObject prefab in the game play with script
        GameObject.DontDestroyOnLoad(CreateNewHolderObject(TrashHolderName).gameObject);
    }

    void OnGUI()
    {
        holders = new List<string>(holderObjects.Keys);
    }

    public static void AddHolderObject(string name, HolderObject holderObject)
    {
        if (!Instance.holderObjects.ContainsKey(name))
        {
            Instance.holderObjects.Add(name, holderObject);
        }else
        {
            Debug.LogWarning($"HolderObject with name {name} already exists in the map.");
        }
    }

    public static HolderObject GetHolderObject(string name)
    {
        if (Instance.holderObjects.TryGetValue(name, out HolderObject holderObject))
        {
            return holderObject;
        }
        else
        {
            Debug.LogWarning($"HolderObject with name {name} not found.");
            return null;
        }
    }

    public static HolderObject CreateNewHolderObject(string name)
    {
        GameObject newHolder = new GameObject(name);
        HolderObject holderObject = newHolder.AddComponent<HolderObject>();
        return holderObject;
    }

    public static void CreateHolderManager()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject holderManagerObject = new GameObject("HolderManager");
        _instance = holderManagerObject.AddComponent<HolderManager>();
        DontDestroyOnLoad(holderManagerObject);
    }
}