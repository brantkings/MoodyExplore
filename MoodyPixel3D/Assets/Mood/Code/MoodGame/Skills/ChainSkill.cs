using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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
        onInterruptSkill = (p, x) =>
        {
            interrupted = x;
        };
        pawn.OnInterruptSkill += onInterruptSkill;

        pawn.UnmarkUsingSkill(this);
        foreach (MoodSkill skill in skills)
        {
            pawn.MarkUsingSkill(skill, skillDirection);
            //Debug.LogFormat("Gonna use skill {0}, {1}", skill, Time.time);
            yield return skill.ExecuteRoutine(pawn, skillDirection);
            //Debug.LogFormat("Finished skill {0}, {1}", skill, Time.time);
            pawn.UnmarkUsingSkill(skill);

            if (interrupted == skill)
            {
                pawn.InterruptSkill(this);
                pawn.MarkUsingSkill(this, skillDirection);
                break;
            }
        }
        pawn.OnInterruptSkill -= onInterruptSkill;
        pawn.MarkUsingSkill(this, skillDirection);
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

    public override bool ShowsBark()
    {
        return false;
    }

    public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
    {
        foreach (MoodSkill skill in skills)
        {
            //float sum = 0f;
            foreach(float time in skill.GetTimeIntervals(pawn, skillDirection))
            {
                yield return time;
            }
        }
    }

    public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection, MoodUnitManager.DistanceBeats distanceSafety)
    {
        return skills.Select((x) => x.WillHaveTarget(pawn, skillDirection, distanceSafety)).Aggregate(
            (x, y) => {
                return (WillHaveTargetResult)Mathf.Max((int)x, (int)y);
            }
        );
    }
}
