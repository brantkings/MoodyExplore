using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodEvent : ScriptableObject
{
    public delegate void DelMoodEvent();

    public event DelMoodEvent OnExecute;
    
    public virtual void Execute()
    {
        Effect();
        OnExecute?.Invoke();
    }

    protected abstract void Effect();
}
