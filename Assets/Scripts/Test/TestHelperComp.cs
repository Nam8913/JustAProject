using UnityEngine;

public class TestComp : EntitiesComp
{
    public override void Init()
    {
        TestProp testProp = this.props as TestProp;
        Debug.Log(testProp.testFloat);
    }
}
