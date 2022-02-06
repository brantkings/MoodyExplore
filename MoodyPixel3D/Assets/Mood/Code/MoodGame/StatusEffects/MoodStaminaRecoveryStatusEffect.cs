using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEf_", menuName = "Mood/Status Effect/Stamina Recovery Reaction", order = 0)]
public class MoodStaminaRecoveryStatusEffect : MoodRoutineStatusEffect
{
    public float totalCumulativeChange = 1f;
    public float onCumulativeChange = 1f;
    public enum Style
    {
        OnlyAcceptPositiveChange,
        OnlyAcceptNegativeChange,
        AllChange,
    }
    public Style changeStyle = Style.AllChange;
    public MoodSkill skillOnCumulativeChange;
    protected override IEnumerator StatusEffectRoutine(MoodPawn pawn)
    {
        float cumulativeChange = 0f;
        float totalChange = 0f;
        float oldStamina = pawn.GetStamina();
        while(true)
        {
            yield return null;
            float currentStamina = pawn.GetStamina();
            float change = currentStamina - oldStamina;
            switch (changeStyle)
            {
                case Style.OnlyAcceptPositiveChange:
                    if (change < 0f) change = 0f;
                    break;
                case Style.OnlyAcceptNegativeChange:
                    if (change > 0f) change = 0f;
                    break;
            }
            change = Mathf.Abs(change);
            cumulativeChange += change;
            totalChange += change;
            Debug.LogFormat("{0} with '{1}' -> {2} to {3} and {4}", pawn.name, GetName(), cumulativeChange, onCumulativeChange, totalChange);
            if(cumulativeChange > onCumulativeChange)
            {
                Debug.LogFormat("{0} can use {1}? {2} {3}", pawn.name, skillOnCumulativeChange.name, pawn.CanUseSkill(skillOnCumulativeChange), pawn.CanUseSkillDebug(skillOnCumulativeChange));
                if (pawn.CanUseSkill(skillOnCumulativeChange))
                {
                    pawn.ExecuteSkill(skillOnCumulativeChange, pawn.Direction, null);
                    cumulativeChange -= onCumulativeChange;
                }
            }
            if(totalChange > totalCumulativeChange)
            {
                pawn.RemoveStatusEffect(this);
                break;
            }
        }
    }


}
