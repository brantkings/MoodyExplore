using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/State", fileName = "Cond_State_")]
public class MoodPawnStateCondition : MoodPawnCondition
{

    private enum DashState
    {
        Dashing,
        NotDashing,
        Indifferent,
    }

    [SerializeField]
    private MoodSkill canUseSkill;
    [SerializeField]
    private DashState dashState = DashState.Indifferent;

    public override bool ConditionIsOK(MoodPawn pawn)
    {
        return CanUseSkillIsOK(pawn) && CanUseSkillIsOK(pawn) && IsDashOK(pawn);
    }

    private bool CanUseSkillIsOK(MoodPawn pawn)
    {
        if (canUseSkill == null) return true;
        else return pawn.CanUseSkill(canUseSkill);
    }


    private bool IsDashOK(MoodPawn pawn)
    {
        switch (dashState)
        {
            case DashState.Dashing:
                return pawn.IsDashing();
            case DashState.NotDashing:
                return !pawn.IsDashing();
            default:
                return true;
        }
    }
}
