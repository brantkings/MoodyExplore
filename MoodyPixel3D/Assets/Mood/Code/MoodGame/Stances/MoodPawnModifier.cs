using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodPawnModifierStamina
{
     void ModifyStamina(ref float stamina, bool moving);
}

public interface IMoodPawnModifierVelocity
{
    void ModifyVelocity(ref Vector3 velocity);
}

public abstract class MoodPawnModifier : ScriptableObject
{
    public void SetModifierApplied(MoodStance stance, MoodPawn pawn, bool set)
    {
        if (set) ApplyModifier(stance, pawn);
        else RemoveModifier(stance, pawn);
    }

    public abstract void ApplyModifier(MoodStance stance, MoodPawn pawn);

    public abstract void RemoveModifier(MoodStance stance, MoodPawn pawn);

    public virtual bool AppliesOnOff
    {
        get => true;
    }
}
