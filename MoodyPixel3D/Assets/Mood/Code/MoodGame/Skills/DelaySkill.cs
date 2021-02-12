using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Delay_", menuName = "Mood/Skill/Delay", order = 0)]
public class DelaySkill : MoodSkill
{
    [Header("Delay")]
    public float delay;
    public MoodPawn.StunType[] stuns;
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
        SolveStun(pawn, true);
        yield return base.ExecuteRoutine(pawn, skillDirection);
        SolveStun(pawn, false);
        DoFeedback(pawn, false);
    }

    private void SolveStun(MoodPawn pawn, bool set)
    {
        if(set)
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

            Debug.LogFormat("Is {0} valid? {1}", triggerAnim, triggerAnim.IsValid());
            if (triggerAnim.IsValid())
                pawn.animator.SetTrigger(triggerAnim);
        }

        pawn.animator.SetBool(boolWhileInSkill, set);
    }

}
