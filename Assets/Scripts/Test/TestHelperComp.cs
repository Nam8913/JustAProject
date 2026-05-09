using UnityEngine;

public class TestHelperComp : HelperComp
{
    public override void Init()
    {
        TestProp testProp = this.props as TestProp;
        Debug.Log(testProp.testFloat);
    }
}
