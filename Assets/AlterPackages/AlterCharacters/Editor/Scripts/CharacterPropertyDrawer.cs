#if UNITY_EDITOR
using Alter.Runtime.Properties;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(CharacterProperty))]
public class CharacaterPropertyDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't indent child fields
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var propertyTypeRect = new Rect(position.x, position.y, 100, position.height);
        SerializedProperty characterTypeProperty = property.FindPropertyRelative("characterType");
        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(propertyTypeRect, characterTypeProperty, GUIContent.none);
        if (characterTypeProperty.enumValueIndex == 1)
        {
            var characterRect = new Rect(position.x + 115, position.y, 100, position.height);
            EditorGUI.PropertyField(characterRect, property.FindPropertyRelative("character"), GUIContent.none);
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
#endif