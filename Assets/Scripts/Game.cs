using UnityEngine;

public abstract class Game : MonoBehaviour
{
    private void Awake()
    { 
        Debug.Log("Game Awake" + this.GetType().Name);
        if(!isGlobalInitialized)
        {
            isGlobalInitialized = true;
            GameService.Ins.GlobalInitialize();
        }
        GameService.Ins.InitializeWhenChangeScene();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        GameService.PlayerInput.Enable();
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
        GameService.PlayerInput.Disable();
    }

    public virtual void OnDestroy()
    {
        
    }

    static bool isGlobalInitialized = false;
}
