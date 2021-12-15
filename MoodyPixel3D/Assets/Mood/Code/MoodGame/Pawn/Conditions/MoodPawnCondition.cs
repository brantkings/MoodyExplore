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
            //Implemented without linq because this is used all the time and is making the game laggy
            case JoinType.All:
                for (int i = 0, len = conditions.Length; i < len; i++)
                {
                    if(!conditions[i].ConditionIsOK(pawn)) return false;
                }
                return true;
            case JoinType.Any:
                for (int i = 0, len = conditions.Length; i < len; i++)
                {
                    if (conditions[i].ConditionIsOK(pawn)) return true;
                }
                return false;
            default:
                return false;
        }
    }

}

