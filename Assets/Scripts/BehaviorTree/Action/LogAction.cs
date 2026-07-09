#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;

public class LogAction : ActionNodeBH
{
    public string logMessage = "LogAction executed.";
    public override void OnEnter()
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log("Entering LogAction");
        #endif
    }

    public override void OnExit()
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log("Exiting LogAction");
        #endif
    }

    public override BHState OnUpdateState()
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log(logMessage);
        #endif
        return BHState.SUCCESS;
    }
}