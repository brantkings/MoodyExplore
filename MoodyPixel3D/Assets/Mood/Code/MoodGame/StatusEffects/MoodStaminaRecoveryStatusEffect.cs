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
    public bool ifChangeIsZeroThenUseNormalChange;
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
            float changeSign = currentStamina - oldStamina;
            oldStamina = currentStamina;
            switch (changeStyle)
            {
                case Style.OnlyAcceptPositiveChange:
                    if (changeSign < 0f) changeSign = 0f;
                    break;
                case Style.OnlyAcceptNegativeChange:
                    if (changeSign > 0f) changeSign = 0f;
                    break;
            }
            float change;
            if (ifChangeIsZeroThenUseNormalChange && changeSign == 0f)
            {
                change = pawn.GetCurrentStaminaRecoverRate() * Time.deltaTime;
            }
            else
            {
                change = Mathf.Abs(changeSign);
            }
            cumulativeChange += change;
            totalChange += change;
            Debug.LogFormat("{0} with '{1}' -> {2} to {3} and {4} (current stamina {5}, {6})", pawn.name, GetName(), cumulativeChange, onCumulativeChange, totalChange, currentStamina.ToString("F2"), changeSign.ToString("F3"));
            if(cumulativeChange > onCumulativeChange)
            {
                Debug.LogFormat("{0} can use {1}? {2} {3}", pawn.name, skillOnCumulativeChange.name, pawn.CanUseSkill(skillOnCumulativeChange), pawn.CanUseSkillDebug(skillOnCumulativeChange));
                Vector3 skillDirection = pawn.Direction;
                if (skillOnCumulativeChange.CanExecute(pawn, skillDirection))
                {
                    pawn.ExecuteSkill(skillOnCumulativeChange, skillDirection, null);
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
