using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Delay_", menuName = "Mood/Skill/Delay", order = 0)]
public class DelaySkill : MoodSkill
{
    [Header("Delay")]
    public float delay;
    public MoodEffectFlag[] flags;

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        foreach (var flag in flags) pawn.AddFlag(flag);
        return delay;
    }
}
