using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableEvent : ScriptableObject
{
    public abstract void Execute(Transform where);

    #if UNITY_EDITOR
    public string tagToTest = "MainCamera";
    [ContextMenu("Test Event")]
    public void Test()
    {
        GameObject obj = GameObject.FindWithTag(tagToTest);
        if(obj != null)
        {
            Execute(GameObject.FindWithTag(tagToTest).transform);
        }
        else
        {
            Debug.LogErrorFormat("Error: Can't find object with tag {0}.", tagToTest);
        }
    }

    #endif
}


public abstract class ScriptableEvent<T> : ScriptableEvent
{
    public override void Execute(Transform where)
    {
        ExecuteReturn(where);
    }

    public abstract T ExecuteReturn(Transform where);

}

public static class ScriptableEventExtensions
{
    public static void Execute(this ScriptableEvent[] collection, Transform where)
    {
        if(collection != null)
        {
            foreach(ScriptableEvent evt in collection) if(evt != null) evt.Execute(where);
        }
    }

    public static void Execute(this List<ScriptableEvent> collection, Transform where)
    {
        if(collection != null)
        {
            foreach(ScriptableEvent evt in collection) if(evt != null) evt.Execute(where);
        }
    }
}