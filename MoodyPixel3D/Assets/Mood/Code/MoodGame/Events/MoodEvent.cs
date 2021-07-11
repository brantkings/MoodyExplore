using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodEvent : ScriptableObject
{
    public delegate void DelMoodEvent(Transform where);

    public event DelMoodEvent OnExecute;
    
    public virtual void Execute(Transform where)
    {
        Effect(where);
        OnExecute?.Invoke(where);
    }


    protected abstract void Effect(Transform where);
}
