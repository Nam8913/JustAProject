#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;

public abstract class Game : MonoBehaviour
{
    private void Awake()
    {
        if(!isGlobalInitialized)
        {
            GameService.Ins.GlobalInitialize();
            isGlobalInitialized = true;
        }
        GameService.Ins.InitializeWhenChangeScene();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        PoolManager.CreatePoolManager();
        HolderManager.CreateHolderManager();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        try
        {
           EventQueue.ProcessNextEvent();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error: {ex} \n {ex.StackTrace}");
        }
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

    static bool isGlobalInitialized = false;
}
