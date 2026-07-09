using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CompProperties), true)]
public class CompPropertiesDrawer : PropertyDrawer
{
    private const float FieldSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        string compClassText = GetCompClassDisplayName(property);
        string fullTypeName = property.managedReferenceFullTypename;

        string className = string.Empty;
        if (!string.IsNullOrEmpty(fullTypeName))
        {
            // Format: "AssemblyName TypeFullName"
            var split = fullTypeName.Split(' ');
            if (split.Length == 2)
            {
                string fullName = split[1];
                className = fullName.Substring(fullName.LastIndexOf('.') + 1);
            }
        }

        if(string.IsNullOrEmpty(className))
        {
            className = "CompProperties (default)";
        }

        Rect headerRect = new Rect(position.x, position.y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(
            headerRect,
            property.isExpanded,
            new GUIContent($"{label.text} : {className}", label.tooltip),
            true);

        if (property.isExpanded)
        {
            float y = position.y + lineHeight + FieldSpacing;

            Rect compClassRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(compClassRect, "compClass", compClassText);
            EditorGUI.EndDisabledGroup();

            y += lineHeight + FieldSpacing;

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            iterator.NextVisible(enterChildren);

            EditorGUI.indentLevel++;
            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect childRect = new Rect(position.x, y, position.width, childHeight);
                EditorGUI.PropertyField(childRect, iterator, true);
                y += childHeight + EditorGUIUtility.standardVerticalSpacing;

                enterChildren = false;
                if (!iterator.NextVisible(enterChildren))
                {
                    break;
                }
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
        {
            return lineHeight;
        }

        float height = lineHeight + FieldSpacing + lineHeight + FieldSpacing;

        SerializedProperty iterator = property.Copy();
        SerializedProperty endProperty = iterator.GetEndProperty();

        bool enterChildren = true;
        iterator.NextVisible(enterChildren);

        while (!SerializedProperty.EqualContents(iterator, endProperty))
        {
            height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;

            enterChildren = false;
            if (!iterator.NextVisible(enterChildren))
            {
                break;
            }
        }

        return height;
    }

    private static string GetCompClassDisplayName(SerializedProperty property)
    {
        // managedReferenceValue chỉ hoạt động với [SerializeReference]
        if (property.propertyType == SerializedPropertyType.ManagedReference)
        {
            if (property.managedReferenceValue is CompProperties compProperties && compProperties.compClass != null)
            {
                return compProperties.compClass.ToString();
            }
        }

        return "null";
    }
}