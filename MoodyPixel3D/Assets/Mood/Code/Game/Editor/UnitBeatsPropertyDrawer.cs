using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MoodUnitManager.UnitBeats), true)]
public class UnitBeatsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty p = property.FindPropertyRelative("beats");
        EditorGUI.PropertyField(position, p, new GUIContent(label.text + $" ({property.type})"));
        EditorGUI.EndProperty();
    }
}
