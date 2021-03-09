using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/Stances", fileName = "Cond_Stance_")]
public class MoodPawnStanceCondition : MoodPawnCondition
{
    public enum What
    {
        ShouldBeOn,
        ShouldBeOff
    }

    [System.Serializable]
    public struct Condition
    {

        public MoodStance stance;
        public What what;

        public bool IsConditionOK(MoodPawn pawn)
        {
            switch (what)
            {
                case What.ShouldBeOn:
                    return pawn.HasStance(stance);
                case What.ShouldBeOff:
                    return !pawn.HasStance(stance);
                default:
                    return true;
            }
        }
    }

    public Condition[] stances;

    public override bool ConditionIsOK(MoodPawn pawn)
    {
        foreach(var cond in stances)
        {
            if (!cond.IsConditionOK(pawn)) return false;
        }
        return true;
    }
}
