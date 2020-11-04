using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableEvent : ScriptableObject
{
    public abstract void Invoke(Transform where);

    #if UNITY_EDITOR
    public string tagToTest = "MainCamera";
    [ContextMenu("Test Event")]
    public void Test()
    {
        GameObject obj = GameObject.FindWithTag(tagToTest);
        if(obj != null)
        {
            Invoke(GameObject.FindWithTag(tagToTest).transform);
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
    public override void Invoke(Transform where)
    {
        InvokeReturn(where);
    }

    public abstract T InvokeReturn(Transform where);

}

public static class ScriptableEventExtensions
{
    public static void Invoke(this ScriptableEvent[] collection, Transform where)
    {
        if(collection != null)
        {
            foreach(ScriptableEvent evt in collection) if(evt != null) evt.Invoke(where);
        }
    }

    public static void Invoke(this List<ScriptableEvent> collection, Transform where)
    {
        if(collection != null)
        {
            foreach(ScriptableEvent evt in collection) if(evt != null) evt.Invoke(where);
        }
    }
}
