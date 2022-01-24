using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodStaminaLeveledChange : MoodPawnLeveledBehaviour, MoodPawn.IStaminaRecoveryDataChanger
{
    


    public MoodPawn.StaminaRecoveryData[] levelIncreases;

    protected override void Initiate(MoodPawn pawn)
    {
    }

    protected override void ApplyLevel(MoodPawn pawn, float level)
    {
        
    }

    public void Change(ref MoodPawn.StaminaRecoveryData data)
    {
        int exactLevel = Mathf.RoundToInt(GetLevel());
        for (int i = 0, len = levelIncreases.Length; i < exactLevel; i++)
        {
            if (i < len)
            {
                data += levelIncreases[i];
            }
            else
            {
                data += levelIncreases[len - 1];
            }
        }
    }
}

