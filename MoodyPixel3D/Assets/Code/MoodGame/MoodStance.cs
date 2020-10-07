using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Mood Stance", fileName = "Stance_")]
public class MoodStance : ScriptableObject
{
    [System.Serializable]
    public class ValueModifier
    {
        public float add = 0f;
        public float multiplier = 1f;

        public void Modify(ref float value)
        {
            value = value * multiplier + add;
        }
    }

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
