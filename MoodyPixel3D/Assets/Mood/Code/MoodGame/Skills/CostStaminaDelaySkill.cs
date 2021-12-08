using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Skill_DelayStamina_", menuName = "Mood/Skill/Delay w Stamina Cost", order = 0)]
public class CostStaminaDelaySkill : StaminaCostMoodSkill
{
    [Header("Delay")]
    public MoodUnitManager.TimeBeats preFeedbackDelay = 0;
    public MoodUnitManager.TimeBeats preDelay = 2;
    public MoodUnitManager.TimeBeats executionDelay = 4;
    public MoodUnitManager.TimeBeats postFeedbackDelay = 0;
    public int plugoutPriorityBefore;
    public int plugoutPriorityAfter;
    public MoodPawn.StunType[] stuns;
    public MoodEffectFlag[] flags;
    public ActivateableMoodStance[] toAdd;

    [Header("Feedback delay")]
    public SoundEffect sfx;
    [SerializeField]
    private AnimatorID triggerAnim;
    [SerializeField]
    private AnimatorID boolWhileInSkill;


    public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.SetPlugoutPriority(plugoutPriorityBefore);
        yield return new WaitForSeconds(preFeedbackDelay);
        DoFeedback(pawn, true);
        SolveStun(pawn, true);
        yield return new WaitForSeconds(preDelay);
        yield return base.ExecuteRoutine(pawn, skillDirection);
        pawn.SetPlugoutPriority(plugoutPriorityAfter);
        SolveStun(pawn, false);
        DoFeedback(pawn, false);
        yield return new WaitForSeconds(postFeedbackDelay);
    }

    private void SolveStun(MoodPawn pawn, bool set)
    {
        if (set)
        {
            for (int i = 0, len = stuns.Length; i < len; i++) pawn.AddStunLock(stuns[i], name);
        }
        else
        {
            for (int i = 0, len = stuns.Length; i < len; i++) pawn.RemoveStunLock(stuns[i], name);
        }
    }

    public override void Interrupt(MoodPawn pawn)
    {
        DoFeedback(pawn, false);
        SolveStun(pawn, false);
        base.Interrupt(pawn);
    }

    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        foreach (var stance in toAdd) pawn.AddStance(stance);
        foreach (var flag in flags) pawn.AddFlag(flag);
        return MergeExecutionResult(base.ExecuteEffect(pawn, skillDirection), (preFeedbackDelay, ExecutionResult.Success));
    }


    private void DoFeedback(MoodPawn pawn, bool set)
    {
        if (pawn.animator != null)
        {
            if (set)
            {
                sfx.ExecuteIfNotNull(pawn.ObjectTransform);
                if (triggerAnim.IsValid())
                    pawn.animator.SetTrigger(triggerAnim);
            }

            pawn.animator.SetBool(boolWhileInSkill, set);
        }
    }

    public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection)
    {
        return WillHaveTargetResult.NonApplicable;
    }

    public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
    {
        yield return preFeedbackDelay.GetLength() + preDelay.GetLength();
        yield return executionDelay.GetLength() + postFeedbackDelay.GetLength();
    }
}
