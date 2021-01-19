using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConditionalMoodStance : MoodStance
{
    [Header("Conditional")]
    [SerializeField]
    private MoodPawnCondition[] conditions;
    [SerializeField]
    private MoodPawnConditionUtils.JoinType howToJoinConditions = MoodPawnConditionUtils.JoinType.All;

    public virtual bool IsItOn(MoodPawn pawn)
    {
        return conditions.ConditionIsOk(pawn, howToJoinConditions);
    }

}
