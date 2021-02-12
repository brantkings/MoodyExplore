using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Utils;
using UnityEditor;


namespace LHH.Utils.Editor
{

    [CustomPropertyDrawer(typeof(LHH.Utils.Internal.TimeStamp<>))]

    public class TimeStampDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("timeAvailable"), label);
            EditorGUI.EndProperty();
        }
    }
}
