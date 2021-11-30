using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/Joint Condition", fileName = "Cond_Union_", order = -1)]
public class MoodPawnUnionCondition : MoodPawnCondition
{
    [System.Serializable]
    private struct ConditionSet
    {
        public MoodPawnCondition[] condition;
        public MoodPawnConditionUtils.JoinType joinType;

        public bool CondititonIsOK(MoodPawn pawn)
        {
            return MoodPawnConditionUtils.ConditionIsOk(condition, pawn, joinType);
        }
    }

    [SerializeField] private ConditionSet[] setsOfConditions;

    public IEnumerable<bool> ListSets(MoodPawn pawn)
    {
        for (int i = 0, len = setsOfConditions.Length; i < len; i++) 
            yield return setsOfConditions[i].CondititonIsOK(pawn);
    }


    public override bool ConditionIsOK(MoodPawn pawn)
    {
        return ListSets(pawn).All((x) => x);
    }
}
