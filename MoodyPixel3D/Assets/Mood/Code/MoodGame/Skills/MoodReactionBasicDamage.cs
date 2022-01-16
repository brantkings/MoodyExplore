using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Skill/Reaction/Basic Damage", fileName = "Reaction_Damage_")]
public class MoodReactionBasicDamage : MoodReaction, IMoodReaction<DamageInfo>
{
    [Header("Basic damage conditions")]
    public bool hasMinDamage;
    public int minDamage;
    public bool hasMaxDamage;
    public int maxDamage = int.MaxValue;

    [Header("Basic damage effect")]
    public float stunDurationMultiplier = 1f;
    public float knockbackDistanceMultiplier = 1f;
    public float knockbackDurationMultiplier = 1f;
    public bool shouldInterruptCurrentSkill;
    public bool dashIsBumpeable = false;

    [Header("Basic damage feedback")]
    public float animationParameterMultiplier = 2f;
    public float animationDelay = 0.125f;
    public AnimationCurve dashCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public virtual bool CanReact(DamageInfo info, MoodPawn pawn)
    {
        return IsAmountOK(info.damage);
    }

    private bool IsAmountOK(int amount)
    {
        return amount == Mathf.Clamp(
            amount,
            hasMinDamage ? minDamage : amount,
            hasMaxDamage? maxDamage: amount
            );
    }

    public virtual void React(ref DamageInfo info, MoodPawn pawn)
    {
        float animationDuration = Mathf.Max(info.stunTime * stunDurationMultiplier, info.durationKnockback * knockbackDistanceMultiplier, animationDelay);

        if (ShouldStaggerAnimation(info))
            pawn.SetDamageAnimationTween(pawn.ObjectTransform.InverseTransformDirection(info.distanceKnockback.normalized) * animationParameterMultiplier, animationDuration - animationDelay, animationDelay);
        //Debug.LogFormat(this,"[REACT] {0} reacting with {1} for {2}. Dash({3}* {4}, {5} * {6})", pawn.name, this, info, info.distanceKnockback, knockbackDistanceMultiplier, info.durationKnockback, knockbackDistanceMultiplier);
        pawn.Dash(info.distanceKnockback * knockbackDistanceMultiplier, measuredInBeats:false, info.durationKnockback * knockbackDurationMultiplier, dashIsBumpeable, dashCurve);
        pawn.RotateDash(info.rotationKnockbackAngle, animationDuration);

        pawn.AddStunLockTimer(MoodPawn.LockType.Action, name, info.stunTime * stunDurationMultiplier);
        if(shouldInterruptCurrentSkill) 
            pawn.InterruptCurrentSkill();
    }

    private bool ShouldStaggerAnimation(DamageInfo info)
    {
        return info.shouldStaggerAnimation;
    }
}
