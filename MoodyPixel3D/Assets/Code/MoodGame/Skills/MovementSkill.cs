using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Skill_Movement_", menuName = "Mood/Skill/Movement", order = 0)]
public class MovementSkill : StaminaCostMoodSkill, RangeArrow.IRangeShowPropertyGiver
{
    public float minDistance;
    public float maxDistance;
    public float velocityAdd = 1f;
    public float durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;
    
    public override IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection)
    {
        Vector3 dist = skillDirection;
        dist = dist.Clamp(minDistance, maxDistance);
        float duration = durationAdd;
        if (velocityAdd != 0f)
        {
            duration += dist.magnitude / velocityAdd;
        }
        pawn.Move(dist, duration, ease);
        yield return new WaitForSecondsRealtime(0.6f);
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
