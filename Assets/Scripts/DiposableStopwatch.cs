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
        UnityEngine.Debug.Log($"{str}: Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
    } 
}
