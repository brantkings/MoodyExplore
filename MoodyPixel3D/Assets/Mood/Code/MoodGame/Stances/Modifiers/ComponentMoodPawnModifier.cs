using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentMoodPawnModifier<T> : MoodPawnModifier where T:Component
{
    public override void ApplyModifier(MoodStance stance, MoodPawn pawn)
    {
        Debug.LogFormat("[MODIFIER] Applying modifier '{0}' from {1} to {2}.", this, stance.name, pawn.name);
        ApplyModifier(stance, GetComponent(pawn));
    }

    public override void RemoveModifier(MoodStance stance, MoodPawn pawn)
    {
        Debug.LogFormat("[MODIFIER] Removing modifier '{0}' from {1} to {2}.", this, stance.name, pawn.name);
        RemoveModifier(stance, GetComponent(pawn));
    }

    public abstract void ApplyModifier(MoodStance stance, T ofPawn);
    public abstract void RemoveModifier(MoodStance stance, T ofPawn);

    public virtual T GetComponent(MoodPawn pawn)
    {
        return pawn.GetComponentInChildren<T>();
    }
}
