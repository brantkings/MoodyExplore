using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LHH.ScriptableObjects.Events;

[CreateAssetMenu(fileName = "Skill_Movement_", menuName = "Mood/Skill/Movement", order = 0)]
public class MovementMoodSkill : StaminaCostMoodSkill, RangeArrow.IRangeShowPropertyGiver
{

    [Header("Movement")]
    [UnityEngine.Serialization.FormerlySerializedAs("minDist")] public MoodUnitManager.DistanceBeats minDistanceBeats;
    [UnityEngine.Serialization.FormerlySerializedAs("maxDist")] public MoodUnitManager.DistanceBeats maxDistanceBeats;
    public MoodUnitManager.DistanceBeats hopHeight;
    public float hopDurationInMultiplier;
    public float hopDurationOutMultiplier;
    public bool movementIsBumpeable = true;
    [SerializeField]
    private bool setHorizontalDirection = true;
    [SerializeField]
    private DirectionFixer[] setDirectionInRelationToMovement;
    public float velocityAdd = 1f;
    public MoodUnitManager.TimeBeats durationAdd;
    public float showArrowWidth = 1f;
    public Ease ease;
    [UnityEngine.Serialization.FormerlySerializedAs("priortyChangeTimeProportional")] public float priorityChangeTimeProportional = 1f;
    public int priorityAdd;
    public MoodUnitManager.TimeBeats postMovementDelay;

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

    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, in CommandData command)
    {
        CalculateMovementData(command.direction, out Vector3 distance, out float duration);
        if(setHorizontalDirection)
        {
            Vector3 setDirection = command.direction;
            SanitizeDirection(pawn.Direction, ref setDirection, setDirectionInRelationToMovement);
            pawn.SetHorizontalDirection(setDirection);
            Debug.LogWarningFormat("Setting {0} direction to {1} -> {2}", pawn, setDirection, pawn.Direction);
        }
        pawn.Dash(distance, measuredInBeats:false, duration, movementIsBumpeable, ease);
        Vector3 argsDirection = command.direction;
        if (HasFeedback())
        {
            pawn.OnNextBeginMove += () => DoFeedback(pawn, argsDirection, true);
            pawn.OnNextEndMove += () => DoFeedback(pawn, argsDirection, false);
        }
        SetFlags(pawn);
        AddStances(pawn);
        var result = base.ExecuteEffect(pawn, command);
        duration += result.Item1 + postMovementDelay;
        if (hopHeight > 0) pawn.Hop(hopHeight, new MoodPawn.TweenData(hopDurationInMultiplier * duration).SetEase(Ease.OutCirc), new MoodPawn.TweenData(hopDurationOutMultiplier * duration).SetEase(Ease.InCirc));
        //Debug.LogWarningFormat("{0} has duration {1}. Distance is {2} and velocity is {3}. Duration real is {4}", this, duration, distance.magnitude, velocityAdd, pawn.CurrentDashDuration());
        pawn.StartCoroutine(PriorityChangeCoroutine(pawn, GetPluginPriority(pawn, this) + priorityAdd, duration * priorityChangeTimeProportional));
        return (duration, ExecutionResult.Success);
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


    private void DoFeedback(MoodPawn pawn, in Vector3 direction, bool set)
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
        distance = skillDirection.Clamp(minDistanceBeats, maxDistanceBeats);
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
            directionFixer = new RangeShow<RangeArrow.Properties>.SkillDirectionSanitizer(minDistanceBeats, maxDistanceBeats),
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
        if (postMovementDelay.beats != 0) yield return postMovementDelay;
    }

    public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection, MoodUnitManager.DistanceBeats distanceSafety)
    {
        return WillHaveTargetResult.NonApplicable;
    }
}
