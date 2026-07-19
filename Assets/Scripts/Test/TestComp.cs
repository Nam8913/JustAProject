#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;
[System.Serializable]
public class TestComp : EntitiesComp
{
    [SerializeField]
    public float callbackValue = 0f;
    public override void Init()
    {
        TestProp testProp = this.props as TestProp;
        #if DEBUG_LOG_FLAG && false
        Debug.Log(testProp.testFloat);
        #endif
        callbackValue = testProp.testFloat;
    }
}
