using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConstantMoodPawnModifier : MoodPawnModifier
{
    public override bool AppliesOnOff => false;

    public override void ApplyModifier(MoodStance stance, MoodPawn pawn)
    {
    }

    public override void RemoveModifier(MoodStance stance, MoodPawn pawn)
    {
    }

}
