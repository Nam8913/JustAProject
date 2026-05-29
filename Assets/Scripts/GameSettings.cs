using System;
using UnityEngine;

public class GameSettings
{
    float screenWidth;
    float screenHeight;
    bool isFullScreen;
    float targetFrameRate;
    bool runInBackgroundable;
    string currFullScreenMode;
    Resolution resolution;
    
    bool isDevMode;

    public GameSettings()
    {
        // Initialize default settings
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        isFullScreen = Screen.fullScreen;
        targetFrameRate = Application.targetFrameRate;
        runInBackgroundable = Application.runInBackground;
        currFullScreenMode = Screen.fullScreenMode.ToString();

        resolution = Screen.currentResolution;

        isDevMode = false;
    }

    public string DebugString()
    {
        return $"Screen Width: {screenWidth}, Screen Height: {screenHeight}, Full Screen: {isFullScreen}, Target Frame Rate: {targetFrameRate}";
    }

    public bool IsDevMode()
    {
        return isDevMode;
    }

    public void SetDevMode(bool value)
    {
        isDevMode = value;
        PublicEventUI.CallBackOnDevMode();
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
