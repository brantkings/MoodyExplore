using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class MoodPawnCondition : ScriptableObject
{
    public abstract bool ConditionIsOK(MoodPawn pawn);
}

public static class MoodPawnConditionUtils
{
    public enum JoinType
    {
        All,
        Any
    }

    public static bool ConditionIsOk(this MoodPawnCondition[] conditions, MoodPawn pawn, JoinType howToJoin = JoinType.All)
    {
        switch (howToJoin)
        {
            case JoinType.All:
                return conditions.All(x => x.ConditionIsOK(pawn));
            case JoinType.Any:
                return conditions.Any(x => x.ConditionIsOK(pawn));
            default:
                return false;
        }
    }

}

