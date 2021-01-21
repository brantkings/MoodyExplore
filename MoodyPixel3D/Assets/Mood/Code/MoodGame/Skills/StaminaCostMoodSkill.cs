using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaminaCostMoodSkill : MoodSkill
{
    [Header("Stamina")]
    [SerializeField]
    private float _cost;

    public virtual float GetStaminaCost()
    {
        return _cost;
    }

    public bool HasPawnEnoughStamina(MoodPawn pawn)
    {
        return pawn.HasStamina(GetStaminaCost());
    }

    public override bool CanExecute(MoodPawn pawn, Vector3 where)
    {
        return HasPawnEnoughStamina(pawn) && base.CanExecute(pawn, where);
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.DepleteStamina(_cost, MoodPawn.StaminaChangeOrigin.Action);
        return 0f;
    }
}
