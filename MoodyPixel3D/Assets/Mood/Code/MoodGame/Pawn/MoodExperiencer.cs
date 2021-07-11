using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodExperiencer : MonoBehaviour
{
    MoodPawn _pawn;

    public delegate void DelExperienceChange(int amount);

    public event DelExperienceChange OnExperienceChange;

    private void Awake()
    {
        _pawn = GetComponentInParent<MoodPawn>();
        if(_pawn == null)
        {
            Debug.LogError("No pawn to compare with in {0}!", this);
            enabled = false;
        }
    }

    internal void GetXP(MoodExperienceGiver origin, int amountXP)
    {
        OnExperienceChange?.Invoke(amountXP);
    }

    private bool CanGetExperience(MoodPawn dead, DamageInfo info)
    {
        return info.origin.GetComponentInParent<MoodPawn>() == _pawn;
    }

}
