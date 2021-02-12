using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Health", fileName = "StMod_Health_")]
public class HealthMoodPawnModifier : ComponentMoodPawnModifier<Health>
{
    public bool makePhased;
    public bool makeInvincible;

    public override Health GetComponent(MoodPawn pawn)
    {
        return pawn.Health;
    }

    public override void ApplyModifier(MoodStance stance, Health health)
    {
        if (makeInvincible) health.SetInvulnerable(true);
        if (makePhased) health.SetInvulnerable(true);
    }

    public override void RemoveModifier(MoodStance stance, Health health)
    {
        if (makeInvincible) health.SetInvulnerable(false);
        if (makePhased) health.SetInvulnerable(false);
    }
}
