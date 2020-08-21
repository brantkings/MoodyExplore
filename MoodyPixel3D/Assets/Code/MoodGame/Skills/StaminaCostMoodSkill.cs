using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaminaCostMoodSkill : MoodSkill
{
    [SerializeField]
    private float _cost;

    protected virtual float GetStaminaCost()
    {
        return _cost;
    }

    public override bool CanExecute(MoodPawn pawn, Vector3 where)
    {
       //Debug.LogFormat("Can {0} execute {1}? {2} and {3}", pawn, this, pawn.HasStamina(GetStaminaCost()),  base.CanExecute(pawn, where));
        return pawn.HasStamina(GetStaminaCost()) && base.CanExecute(pawn, where);
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.DepleteStamina(_cost);
        return 0f;
    }
}
