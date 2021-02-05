using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Skill_Movement_", menuName = "Mood/Skill/Movement", order = 0)]
public class MovementMoodSkill : StaminaCostMoodSkill, RangeArrow.IRangeShowPropertyGiver
{
    [Header("Movement")]
    public float minDistance;
    public float maxDistance;
    public float hopHeight;
    public float hopDuration;
    [SerializeField]
    private bool setHorizontalDirection = true;
    [SerializeField]
    private DirectionFixer[] setDirectionInRelationToMovement;
    public float velocityAdd = 1f;
    public float durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;

    [Header("Feedback Movement")]
    public SoundEffect sfx;
    public ActivateableMoodStance[] toAdd;
    [SerializeField]
    private AnimatorID triggerAnim;
    [SerializeField]
    private AnimatorID boolWhileInSkill;

    [Header("Flags")]
    public MoodEffectFlag[] onBeginningFlags;
    public MoodEffectFlag[] onEndFlags;
    public MoodEffectFlag[] onCompleteFlags;

    private void AddStances(MoodPawn pawn)
    {
        foreach(ActivateableMoodStance s in toAdd)
        {
            pawn.AddStance(s);
        }
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        CalculateMovementData(skillDirection, out Vector3 distance, out float duration);
        if(setHorizontalDirection)
        {
            Vector3 setDirection = skillDirection;
            SanitizeDirection(pawn.Direction, ref setDirection, setDirectionInRelationToMovement);
            pawn.SetHorizontalDirection(setDirection);
        }
        pawn.Dash(distance, duration, ease);
        if(HasFeedback())
        {
            pawn.OnNextBeginMove += () => DoFeedback(pawn, true);
            pawn.OnNextEndMove += () => DoFeedback(pawn, false);
        }
        SetFlags(pawn);
        AddStances(pawn);
        sfx.ExecuteIfNotNull(pawn.ObjectTransform);
        duration += base.ExecuteEffect(pawn, skillDirection);
        return duration;
    }

    private bool HasFeedback()
    {
        return triggerAnim.IsValid() || boolWhileInSkill.IsValid();
    }


    private void DoFeedback(MoodPawn pawn, bool set)
    {
        if(set)
        {
            if(triggerAnim.IsValid())
                pawn.animator.SetTrigger(triggerAnim);
        }

        pawn.animator.SetBool(boolWhileInSkill, set);
    }

    private void SetFlags(MoodPawn pawn)
    {
        pawn.AddFlags(onBeginningFlags);
        if(onEndFlags != null && onEndFlags.Length > 0)
            pawn.OnNextEndMove += () => {
                pawn.AddFlags(onEndFlags);
                };
        if (onCompleteFlags != null && onCompleteFlags.Length > 0)
            pawn.OnNextCompleteMove += () => {
                pawn.AddFlags(onCompleteFlags);
                };
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

    public override IEnumerable<MoodStance> GetStancesThatWillBeAdded()
    {
        foreach (var stance in toAdd) yield return stance;
    }
}
