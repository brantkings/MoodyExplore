using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Skill_Chain_", menuName = "Mood/Skill/Chain of Skills", order = 0)]
public class ChainSkill : StaminaCostMoodSkill
{
    [Header("Chain")]
    public MoodSkill[] skills;

    public override float GetStaminaCost()
    {
        float cost = base.GetStaminaCost();
        foreach(MoodSkill skill in skills)
        {
            StaminaCostMoodSkill staminaCost = skill as StaminaCostMoodSkill;
            if(staminaCost != null)
            {
                cost += staminaCost.GetStaminaCost();
            }
        }
        return cost;
    }

    public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.DepleteStamina(base.GetStaminaCost(), MoodPawn.StaminaChangeOrigin.Action);

        MoodPawn.DelMoodPawnUndirectedSkillEvent onInterruptSkill = default(MoodPawn.DelMoodPawnUndirectedSkillEvent);
        bool interrupted = false;
        onInterruptSkill = (x) =>
        {
            //Debug.LogFormat("Interrupted! {0}", this);
            interrupted = true;
        };
        pawn.OnInterruptSkill += onInterruptSkill;

        foreach (MoodSkill skill in skills)
        {
            pawn.UnmarkUsingSkill(this);
            pawn.MarkUsingSkill(skill);
            yield return skill.ExecuteRoutine(pawn, skillDirection);
            pawn.UnmarkUsingSkill(skill);

            if (interrupted)
            {
                Interrupt(pawn);
                break;
            }
            pawn.MarkUsingSkill(this);
        }
        pawn.OnInterruptSkill -= onInterruptSkill;
    }


    public override void Interrupt(MoodPawn pawn)
    {
        base.Interrupt(pawn);
    }

    public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
    {
        foreach (MoodSkill skill in skills)
        {
            skill.SetShowDirection(pawn, direction);
        }
    }
}
