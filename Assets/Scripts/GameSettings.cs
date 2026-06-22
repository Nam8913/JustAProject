using System;
using UnityEngine;
[Serializable]
public class GameSettings
{
    float screenWidth;
    float screenHeight;
    bool isFullScreen;
    float targetFrameRate;
    bool runInBackgroundable;
    string currFullScreenMode;

    [NonSerialized]
    Resolution resolution;
    
    bool isDevMode;

    public GameSettings()
    {
        resolution = Screen.currentResolution;

        // Initialize default settings
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        isFullScreen = Screen.fullScreen;
        targetFrameRate = Application.targetFrameRate;
        runInBackgroundable = Application.runInBackground;
        currFullScreenMode = Screen.fullScreenMode.ToString();

        isDevMode = false;
    }

    public string DebugString()
    {
        return String.Concat(
            $"Screen Width: {screenWidth}\n",
            $"Screen Height: {screenHeight}\n",
            $"Is Full Screen: {isFullScreen}\n",
            $"Target Frame Rate: {targetFrameRate}\n",
            $"Run In Background: {runInBackgroundable}\n",
            $"Current Full Screen Mode: {currFullScreenMode}\n",
            $"Resolution: {resolution.width} x {resolution.height} @ {resolution.refreshRateRatio}Hz\n",
            $"Is Dev Mode: {isDevMode}\n"
        );
    }

    public bool IsDevMode()
    {
        return isDevMode;
    }

    public void SetDevMode(bool value)
    {
        isDevMode = value;
        EntryUI.CallBackOnDevMode();
    }

    public bool IsFullScreen()
    {
        return isFullScreen;
    }

    public void SetFullScreen(bool value)
    {
        if(Screen.fullScreenMode != FullScreenMode.Windowed)
        {
            return;
        }
        Screen.fullScreen = value;
        isFullScreen = Screen.fullScreen;
    }

    public string GetCurrentFullScreenMode()
    {
        return currFullScreenMode;
    }

    public void SetFullscreenWindow()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        currFullScreenMode = Screen.fullScreenMode.ToString();
    }

    public void SetExclusiveFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        currFullScreenMode = Screen.fullScreenMode.ToString();
    }

    public void SetWindowed()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        currFullScreenMode = Screen.fullScreenMode.ToString();
    }

    public void SetMaximized()
    {
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        currFullScreenMode = Screen.fullScreenMode.ToString();
    }

    public float GetTargetFrameRate()
    {
        return targetFrameRate;
    }

    public void SetTargetFrameRate(float frameRate)
    {
        Application.targetFrameRate = (int)frameRate;
        targetFrameRate = Application.targetFrameRate;
    }

    public bool IsRunInBackground()
    {
        return runInBackgroundable;
    }

    public void SetRunInBackgroundable(bool value)
    {
        Application.runInBackground = value;
        runInBackgroundable = Application.runInBackground;
    }

    public string[] showAvailableResolutions()
    {
        Resolution[] availableResolutions = Screen.resolutions;
        string[] resStrings = new string[availableResolutions.Length];
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            resStrings[i] = $"{availableResolutions[i].width} x {availableResolutions[i].height} @ {availableResolutions[i].refreshRateRatio}Hz";
        }
        return resStrings;
    }

    public Resolution GetCurrentResolution()
    {
        return resolution;
    }
}
