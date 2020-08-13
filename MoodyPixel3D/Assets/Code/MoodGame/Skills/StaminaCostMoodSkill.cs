using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaminaCostMoodSkill : MoodSkill
{
    [SerializeField]
    private float _cost;

    protected virtual float GetCost()
    {
        return _cost;
    }

    protected bool HaveEnoughStamina()
    {
        return GetCost() < GetCurrentStamina();
    }

    protected float GetCurrentStamina()
    {
        return float.PositiveInfinity;
    }
    
    public override bool CanExecute(MoodPawn pawn, Vector3 where)
    {
        return HaveEnoughStamina() && base.CanExecute(pawn, where);
    }
}
