using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodReaction<T>
{
    public abstract bool CanReact(T info, MoodPawn pawn);
    public abstract void React(ref T info, MoodPawn pawn);
}

public abstract class MoodReaction : ScriptableObject
{
    public enum ActionType
    {
        Damage,
        Bump,
        Bumped,
        All
    }

    public virtual int Priority
    {
        get
        {
            return 0;
        }
    }
}
