using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[CreateAssetMenu(menuName = "Mood/Stances/Conditional Mood Stance", fileName = "Stance_C_")]
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
