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
        MoodSkill interrupted = null;
        onInterruptSkill = (x) =>
        {
            Debug.LogErrorFormat("Interrupted! {0} and {1}", this, x);
            interrupted = x;
        };
        pawn.OnInterruptSkill += onInterruptSkill;

        foreach (MoodSkill skill in skills)
        {
            pawn.UnmarkUsingSkill(this);
            pawn.MarkUsingSkill(skill);
            Debug.LogFormat("Gonna use skill {0}, {1}", skill, Time.time);
            yield return skill.ExecuteRoutine(pawn, skillDirection);
            Debug.LogFormat("Finished skill {0}, {1}", skill, Time.time);
            pawn.UnmarkUsingSkill(skill);

            pawn.MarkUsingSkill(this);
            if (interrupted == skill)
            {
                pawn.InterruptSkill(this);
                break;
            }
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

    public override bool ImplementsRangeShow<T>()
    {
        foreach (MoodSkill skill in skills) if (skill.ImplementsRangeShow<T>()) return true;
        return base.ImplementsRangeShow<T>();
    }

    public override RangeShow<T>.IRangeShowPropertyGiver GetRangeShowProperty<T>()
    {
        foreach (MoodSkill skill in skills) if (skill.ImplementsRangeShow<T>()) return skill.GetRangeShowProperty<T>();
        return base.GetRangeShowProperty<T>();
    }
}
