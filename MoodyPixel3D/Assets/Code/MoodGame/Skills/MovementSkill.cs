using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Skill_Movement_", menuName = "Mood/Skill/Movement", order = 0)]
public class MovementSkill : StaminaCostMoodSkill, IRangeArrowSkill
{
    public float minDistance;
    public float maxDistance;
    public float velocityAdd = 1f;
    public float durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;
    
    public override void Execute(MoodPawn pawn, Vector3 skillDirection)
    {
        Vector3 dist = skillDirection;
        dist = dist.Clamp(minDistance, maxDistance);
        float duration = durationAdd;
        if (velocityAdd != 0f)
        {
            duration += dist.magnitude / velocityAdd;
        }
        pawn.Move(dist, duration, ease);
    }

    public RangeArrow.Properties GetRangeArrowProperties()
    {
        return new RangeArrow.Properties()
        {
            minLength = minDistance,
            maxLength = maxDistance,
            width = showArrowWidth
        };
    }
}
