using System;
using System.Diagnostics;
using UnityEngine;

public class DisposableStopwatch : IDisposable
{
    private Stopwatch stopwatch;

    public DisposableStopwatch()
    {
        stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
    } 
}
