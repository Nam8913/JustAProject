using UnityEngine;

public class PublicEventUI : MonoBehaviour
{
    static PublicEventUI ins;
    static string nameHolderObj = string.Empty;
    [SerializeField]private Canvas canvas;
    [SerializeField]private GameObject newGameWindow;
    [SerializeField]private GameObject loadGameWindow;
    [SerializeField]private GameObject modsWindow;
    [SerializeField]private GameObject settingWindow;
    [SerializeField]private GameObject devToolWindow;

    [Header("Dev_Tools")]
    [SerializeField]private GameObject devWindowButton;

    void Awake()
    {
        if(ins != null)
        {
            Debug.LogError("Multiple instances of PublicEventUI detected. There should only be one instance of PublicEventUI in the scene.");
            Destroy(this);
            return;
        }
        ins = this;
        nameHolderObj = ins.gameObject.name;
    }

    void Start()
    {
        canvas = GameService.Ins.Canvas;

        if(newGameWindow != null)
        {
            newGameWindow.SetActive(false);
        }
        if(loadGameWindow != null)
        {
            loadGameWindow.SetActive(false);
        }
        if(modsWindow != null)
        {
            modsWindow.SetActive(false);
        }
        if(settingWindow != null)
        {
            settingWindow.SetActive(false);
        }
        if(devToolWindow != null)
        {
            devToolWindow.SetActive(false);
        }

        CallBackOnDevMode();
    }

    public static void CallBackOnDevMode()
    {
        if(ins == null)
        {
            return;
        }
        if(GameService.Ins.Settings.IsDevMode())
        {
            ins.devWindowButton.SetActive(true);
        }else
        {
            ins.devWindowButton.SetActive(false);
        }
    }

    public void TriggerNewGameButton()
    {
        bool isNewGameWindowActive = newGameWindow.activeSelf;
        if(!isNewGameWindowActive)
        {
            newGameWindow.SetActive(true);
        }else
        {
            newGameWindow.SetActive(false);
        }
    }
    public void TriggerLoadGameButton()
    {
        bool isLoadGameWindowActive = loadGameWindow.activeSelf;
        if(!isLoadGameWindowActive)
        {
            loadGameWindow.SetActive(true);
        }else
        {
            loadGameWindow.SetActive(false);
        }
    }

    public void TriggerModsWindowButton()
    {
        bool isModsWindowActive = modsWindow.activeSelf;
        if(!isModsWindowActive)
        {
            modsWindow.SetActive(true);
        }else
        {
            modsWindow.SetActive(false);
        }
    }

    public void TriggerSettingsButton()
    {
        bool isSettingWindowActive = settingWindow.activeSelf;
        if(!isSettingWindowActive)
        {
            settingWindow.SetActive(true);
        }else
        {
            settingWindow.SetActive(false);
        }
    }

    public void TriggerDevToolButton()
    {
        bool isDevToolWindowActive = devToolWindow.activeSelf;
        if(!isDevToolWindowActive)
        {
            devToolWindow.SetActive(true);
        }else
        {
            devToolWindow.SetActive(false);
        }
    }

    public void TriggerExitsButton()
    {
        SceneHandler.QuitGame();
    }
}
