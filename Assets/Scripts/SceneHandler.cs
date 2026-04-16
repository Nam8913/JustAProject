using UnityEngine;
using UnityEngine.SceneManagement;
public static class SceneHandler
{
    private static string currentScene;
    private const string PlayScene = "Play";
    private const string EntryScene = "Entry";

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void LoadScene(string sceneName)
    {
        if (currentScene != sceneName)
        {
            SceneManager.LoadScene(sceneName);
            currentScene = sceneName;
        }
    }

    public static void LoadPlayScene()
    {
        if (currentScene != PlayScene)
        {
           SceneManager.LoadScene(PlayScene);
            currentScene = PlayScene;
        }
    }
    public static void LoadEntryScene()
    {
        if (currentScene != EntryScene)
        {
            SceneManager.LoadScene(EntryScene);
            currentScene = EntryScene;
        }
    }
    public static string CurrentScene
    {
        get
        {
            if(string.IsNullOrEmpty(currentScene))
            {
                currentScene = SceneManager.GetActiveScene().name;
            }
            return currentScene;
        }
    }

    public static bool IsPlayScene()
    {
        return CurrentScene == PlayScene;
    }

    public static bool IsEntryScene()
    {
        return CurrentScene == EntryScene;
    }
}
