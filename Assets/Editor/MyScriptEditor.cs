using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Test))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ Inspector mặc định
        DrawDefaultInspector();

        // Lấy reference tới script
        Test test = (Test)target;

        // Tạo button
        if (GUILayout.Button("Do Something"))
        {
            test.DoSomeThing();
        }
    }
}