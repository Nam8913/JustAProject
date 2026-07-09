#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;

public class TestComp : EntitiesComp
{
    public override void Init()
    {
        TestProp testProp = this.props as TestProp;
        #if DEBUG_LOG_FLAG && false
        Debug.Log(testProp.testFloat);
        #endif
    }
}
