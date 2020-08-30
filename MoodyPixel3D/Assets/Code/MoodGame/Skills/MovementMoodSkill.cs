using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Skill_Movement_", menuName = "Mood/Skill/Movement", order = 0)]
public class MovementMoodSkill : StaminaCostMoodSkill, RangeArrow.IRangeShowPropertyGiver
{
    public float minDistance;
    public float maxDistance;
    public float velocityAdd = 1f;
    public float durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;


    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        CalculateMovementData(skillDirection, out Vector3 distance, out float duration);
        pawn.SetHorizontalDirection(skillDirection);
        pawn.Dash(distance, duration, ease);
        duration += base.ExecuteEffect(pawn, skillDirection);
        return duration;
    }

    private void CalculateMovementData(Vector3 skillDirection, out Vector3 distance, out float duration)
    {
        distance = skillDirection;
        distance = distance.Clamp(minDistance, maxDistance);
        duration = durationAdd;
        if (velocityAdd != 0f)
        {
            duration += distance.magnitude / velocityAdd;
        }
    }
    

    public RangeArrow.Properties GetRangeProperty()
    {
        return new RangeArrow.Properties()
        {
            minLength = minDistance,
            maxLength = maxDistance,
            width = showArrowWidth
        };
    }
}
