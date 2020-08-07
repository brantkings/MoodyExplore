using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GetterFromGameObjects), true)]
public class InterfaceGetterPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        GetterFromGameObjects getter = fieldInfo.GetValue(property.serializedObject.targetObject) as GetterFromGameObjects;
        if (getter != null)
        {
            return GetHeightFromLines(property, label, getter.GetAmountOfLinesToReport());
        }
        else
        {
            return GetHeightFromLines(property, label, 0);
        }
    }

    public float GetHeightFromLines(SerializedProperty property, GUIContent label, int amountOfLines)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("toGetFrom")) + 10f * (1 + amountOfLines);
    }

    public int GetAmountOfLinesTotal(IEnumerable<GetterFromGameObjects> getters)
    {
        int amount = 0;
        foreach (var get in getters) amount += get.GetAmountOfLinesToReport();
        return amount;
    }

    GetterFromGameObjects[] arrayCache;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUIStyle style = new GUIStyle();
        style.richText = true;

        float lineSize = base.GetPropertyHeight(property, label);
        Rect objectRect = position;
        objectRect.size = new Vector2(position.size.x, lineSize);
        SerializedProperty objectArrayProp = property.FindPropertyRelative("toGetFrom");
        if (!EditorGUI.PropertyField(objectRect, objectArrayProp, label, true))
        {
            objectRect.y += EditorGUI.GetPropertyHeight(objectArrayProp);
        }
        else
        {
            objectRect.y += lineSize;
        }
        GetterFromGameObjects getter = fieldInfo.GetValue(property.serializedObject.targetObject) as GetterFromGameObjects;
        if(getter != null)
        {
            arrayCache = null;
            objectRect.size = new Vector2(objectRect.size.x, lineSize * getter.GetAmountOfLinesToReport());
            EditorGUI.LabelField(objectRect, string.Format("<color=#8886b9>{0}</color>",getter.ReportFromObject()), style);
        } 
    }
}
