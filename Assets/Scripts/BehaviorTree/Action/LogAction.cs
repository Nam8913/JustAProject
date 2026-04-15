using UnityEngine;

public class LogAction : ActionNodeBH
{
    public string logMessage = "LogAction executed.";
    public override void OnEnter()
    {
        Debug.Log("Entering LogAction");
    }

    public override void OnExit()
    {
        Debug.Log("Exiting LogAction");
    }

    public override BHState OnUpdateState()
    {
        Debug.Log(logMessage);
        return BHState.SUCCESS;
    }
}