using UnityEngine;

public class PublicEventUI : MonoBehaviour
{
    [SerializeField]private Canvas canvas;
    [SerializeField]private GameObject newGameWindow;
    [SerializeField]private GameObject loadGameWindow;
    [SerializeField]private GameObject modsWindow;
    [SerializeField]private GameObject settingWindow;
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

    public void TriggerExitsButton()
    {
        SceneHandler.QuitGame();
    }
}
