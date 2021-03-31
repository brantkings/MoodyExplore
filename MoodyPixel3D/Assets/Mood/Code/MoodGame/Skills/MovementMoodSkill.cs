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
    public float hopDurationInMultiplier;
    public float hopDurationOutMultiplier;
    [SerializeField]
    private bool setHorizontalDirection = true;
    [SerializeField]
    private DirectionFixer[] setDirectionInRelationToMovement;
    public float velocityAdd = 1f;
    public float durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;
    public float priortyChangeTimeProportional = 1f;
    public int priorityAdd;

    [Header("Feedback Movement")]
    public bool warningOnBumpWall;
    public ScriptableEvent[] eventsOnStart;
    public ScriptableEvent[] eventsOnEnd;
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
        if (HasFeedback())
        {
            pawn.OnNextBeginMove += () => DoFeedback(pawn, skillDirection, true);
            pawn.OnNextEndMove += () => DoFeedback(pawn, skillDirection, false);
        }
        SetFlags(pawn);
        AddStances(pawn);
        duration += base.ExecuteEffect(pawn, skillDirection);
        if (hopHeight > 0) pawn.FakeHop(hopHeight, hopDurationInMultiplier * duration, hopDurationOutMultiplier * duration);
        //Debug.LogWarningFormat("{0} has duration {1}. Distance is {2} and velocity is {3}. Duration real is {4}", this, duration, distance.magnitude, velocityAdd, pawn.CurrentDashDuration());
        pawn.StartCoroutine(PriorityChangeCoroutine(pawn, GetPluginPriority(pawn) + priorityAdd, duration * priortyChangeTimeProportional));
        return duration;
    }

    private IEnumerator PriorityChangeCoroutine(MoodPawn pawn, int changeTo, float duration)
    {
        if(duration > 0)
            yield return new WaitForSeconds(duration);
        pawn.SetPlugoutPriority(changeTo);
    }

    private bool HasFeedback()
    {
        return triggerAnim.IsValid() || boolWhileInSkill.IsValid() || eventsOnStart.Length > 0 || eventsOnEnd.Length > 0;
    }


    private void DoFeedback(MoodPawn pawn, Vector3 direction, bool set)
    {
        if(pawn.animator != null)
        {
            if(set)
            {
                eventsOnStart.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(direction));
                if (triggerAnim.IsValid())
                    pawn.animator.SetTrigger(triggerAnim);
            }
            else
            {
                eventsOnEnd.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(direction));
            }

            pawn.animator.SetBool(boolWhileInSkill, set);
        }
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
        distance = skillDirection.Clamp(minDistance, maxDistance);
        duration = durationAdd;
        if (velocityAdd != 0f)
        {
            duration += distance.magnitude / velocityAdd;
        }
    }

    private float GetDurationSpecialEffect()
    {
        float dur = 0;
        foreach(var stance in GetStancesThatWillBeAdded())
        {
            ActivateableMoodStance aStance = stance as ActivateableMoodStance;
            if(aStance != null)
            {
                dur = Mathf.Max(aStance.GetTimeoutDelay(), dur);
            }
        }
        return dur;
    }


    public RangeArrow.Properties GetRangeProperty()
    {
        return new RangeArrow.Properties()
        {
            directionFixer = new RangeShow<RangeArrow.Properties>.SkillDirectionSanitizer(minDistance, maxDistance),
            width = showArrowWidth,
            warningOnHit = this.warningOnBumpWall,
            effectDuration = GetDurationSpecialEffect(),
            velocityInfo = new RangeArrow.Properties.VelocityInfo(velocityAdd, durationAdd)
        };
    }

    public override IEnumerable<MoodStance> GetStancesThatWillBeAdded()
    {
        foreach (var stance in base.GetStancesThatWillBeAdded()) yield return stance;
        foreach (var stance in toAdd) yield return stance;
    }

    public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
    {
        CalculateMovementData(skillDirection, out Vector3 distance, out float duration);
        yield return duration;
    }
}
