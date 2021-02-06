using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Delay_", menuName = "Mood/Skill/Delay", order = 0)]
public class DelaySkill : MoodSkill
{
    [Header("Delay")]
    public float delay;
    public MoodEffectFlag[] flags;

    [Header("Feedback delay")]
    public SoundEffect sfx;
    public ActivateableMoodStance[] toAdd;
    [SerializeField]
    private AnimatorID triggerAnim;
    [SerializeField]
    private AnimatorID boolWhileInSkill;


    public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        DoFeedback(pawn, true);
        yield return base.ExecuteRoutine(pawn, skillDirection);
        DoFeedback(pawn, false);
    }

    public override void Interrupt(MoodPawn pawn)
    {
        DoFeedback(pawn, false);
        base.Interrupt(pawn);
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        foreach (var stance in toAdd) pawn.AddStance(stance);
        foreach (var flag in flags) pawn.AddFlag(flag);
        return delay;
    }


    private void DoFeedback(MoodPawn pawn, bool set)
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
