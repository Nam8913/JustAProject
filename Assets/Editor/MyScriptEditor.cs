using System.Reflection;
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

        MethodInfo[] methods = target.GetType().GetMethods(
            BindingFlags.Instance | 
            BindingFlags.Public | 
            BindingFlags.NonPublic
        );

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<MakeButtonFuncOnTestClassAttribute>();

            if (attr != null)
            {
                string buttonName = method.Name;
                
                if(attr.isWorkOnlyInRuntime)
                {
                    buttonName += " (Runtime Only)";
                    GUI.enabled = Application.isPlaying;
                }else
                {
                    buttonName += " (Editor Only)";
                    GUI.enabled = Application.isEditor && !Application.isPlaying;
                }

                if (GUILayout.Button(buttonName))
                {
                    method.Invoke(test, null);
                }
            }
        }

        GUI.enabled = true;
    }
}