using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodStaminaLeveledChange : MoodPawnLeveledBehaviour
{
    [System.Serializable]
    public struct Rate
    {
        public float idle;
        public float moving;

        public static Rate operator+(Rate a, Rate b)
        {
            return new Rate()
            {
                idle = a.idle + b.idle,
                moving = a.moving + b.moving
            };
        }

        public static Rate operator-(Rate a, Rate b)
        {
            return new Rate()
            {
                idle = a.idle - b.idle,
                moving = a.moving - b.moving
            };
        }

        public static Rate operator -(Rate a)
        {
            return new Rate()
            {
                idle = -a.idle,
                moving = -a.moving
            };
        }
    }

    private Rate initial;

    public Rate[] levelIncreases;

    protected override void Initiate(MoodPawn pawn)
    {
        initial.idle = pawn.staminaRecoveryIdlePerSecond;
        initial.moving = pawn.staminaRecoveryMovingPerSecond;
    }

    protected override void ApplyLevel(MoodPawn pawn, float level)
    {
        int exactLevel = Mathf.RoundToInt(level);
        Rate rate = initial;
        for (int i = 0, len = levelIncreases.Length; i < exactLevel; i++)
        {
            if(i<len)
            {
                rate += levelIncreases[i];
            }
            else
            {
                rate += levelIncreases[len - 1];
            }
        }

        pawn.staminaRecoveryIdlePerSecond = rate.idle;
        pawn.staminaRecoveryMovingPerSecond = rate.moving;
    }
}

