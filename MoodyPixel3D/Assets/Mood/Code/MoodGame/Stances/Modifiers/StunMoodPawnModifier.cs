using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Stun", fileName = "StMod_Stun")]
public class StunMoodPawnModifier : MoodPawnModifier
{
    public MoodPawn.StunType[] stuns;
    public bool useStanceName;

    public override void ApplyModifier(MoodStance stance, MoodPawn pawn)
    {
        for (int i = 0, len = stuns.Length; i < len; i++) pawn.AddStunLock(stuns[i], GetID(stance));
    }

    public override void RemoveModifier(MoodStance stance, MoodPawn pawn)
    {
        for (int i = 0, len = stuns.Length; i < len; i++) pawn.RemoveStunLock(stuns[i], GetID(stance));
    }

    private string GetID(MoodStance stance)
    {
        if (useStanceName) return stance.name;
        else return name;
    }
}
