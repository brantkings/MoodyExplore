using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Stamina", fileName = "StMod_Stamina_")]
public class StaminaMoodPawnModifier : ConstantMoodPawnModifier, IMoodPawnModifierStamina
{
    [SerializeField]
    private ValueModifier staminaOverTimeIdle;
    [SerializeField]
    private ValueModifier staminaOverTimeMoving;

    public void ModifyStamina(ref float stamina, bool moving)
    {
        if (moving) staminaOverTimeMoving.Modify(ref stamina);
        else staminaOverTimeIdle.Modify(ref stamina);
    }
}
