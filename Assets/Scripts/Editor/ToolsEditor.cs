using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Tools))]
public class ToolsEditor : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(new Rect(position.x, position.y, 50, 370), label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var target = property.serializedObject.targetObject;

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var nameRect = new Rect(position.x, position.y, 70, position.height);
        var headRect = new Rect(position.x + 50, position.y , 120, position.height);
        var toolTypeRect = new Rect(position.x + 150, position.y, 80, position.height);
        var splatterTypeRect = new Rect(position.x + 210, position.y, 80, position.height);
        var textureRect = new Rect(position.x + 270, position.y, 120, position.height);
        var IconRect =  new Rect(position.x + 370, position.y, 120, position.height);

        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(headRect, property.FindPropertyRelative("head"), GUIContent.none);
        EditorGUI.PropertyField(toolTypeRect, property.FindPropertyRelative("toolType"), GUIContent.none);

        EditorGUI.PropertyField(splatterTypeRect, property.FindPropertyRelative("splatterType"), GUIContent.none);
        EditorGUI.PropertyField(textureRect, property.FindPropertyRelative("texture"), GUIContent.none);
        EditorGUI.PropertyField(IconRect, property.FindPropertyRelative("icon"), GUIContent.none);
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}