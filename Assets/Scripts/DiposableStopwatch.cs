#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Diagnostics;
using UnityEngine;

public class DisposableStopwatch : IDisposable
{
    private string str = "-";
    private Stopwatch stopwatch;

    public DisposableStopwatch(string str = "-")
    {
        this.str = str;
        stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        stopwatch.Stop();
        #if DEBUG_LOG_FLAG && true
        UnityEngine.Debug.LogWarning($"{str}: Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        #endif
    } 
}
