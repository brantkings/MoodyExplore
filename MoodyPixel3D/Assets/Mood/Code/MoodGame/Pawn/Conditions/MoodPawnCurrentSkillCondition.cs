using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/Current Skill State", fileName = "Cond_CurrentSkill_")]
public class MoodPawnCurrentSkillCondition : MoodPawnCondition
{
    public enum NameCond
    {
        Contains,
        NotContains,
        Indifferent
    }

    private enum SkillState
    {
        UsingSkill,
        NotUsingSkill,
        Indiferrent,
    }

    [System.Serializable]
    private struct NameCondition
    {

        public string toCompare;
        public NameCond condition;

        public bool IsConditionOk(string skillName)
        {
            switch (condition)
            {
                case NameCond.Contains:
                    return skillName.Contains(toCompare);
                case NameCond.NotContains:
                    return !skillName.Contains(toCompare);
                default:
                    return true;
            }
        }
    }

    [SerializeField]
    private SkillState skillState = SkillState.Indiferrent;

    [SerializeField]
    private NameCondition[] nameConditions;

    public override bool ConditionIsOK(MoodPawn pawn)
    {
        if (!IsSkillOK(pawn)) return false;

        string str = pawn.GetCurrentSkill()?.name;
        if(!string.IsNullOrEmpty(str))
        {
            foreach(var c in nameConditions)
            {
                if (!c.IsConditionOk(str)) return false;
            }
        }

        return true;
    }


    private bool IsSkillOK(MoodPawn pawn)
    {
        switch (skillState)
        {
            case SkillState.UsingSkill:
                return pawn.IsExecutingSkill();
            case SkillState.NotUsingSkill:
                return !pawn.IsExecutingSkill();
            default:
                return true;
        }
    }


}
