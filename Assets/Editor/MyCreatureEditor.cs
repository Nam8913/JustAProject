using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Creature))]
public class MyCreatureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Creature myCreature = (Creature)target;

        if (GUILayout.Button("Do Something"))
        {
            DebugCreature(myCreature);
        }
    }

    void DebugCreature(Creature target)
    {
        Debug.Log("============ Debugging Creature ===========");
        Debug.Log($"Name: {target.LabelName}");
        Debug.Log($"Description: {target.LabelDescription}");
        target.ConfigError();
    }
}