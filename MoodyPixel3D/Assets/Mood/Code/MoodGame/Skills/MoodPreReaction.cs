using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;



[CreateAssetMenu(menuName = "Mood/Skill/General Reaction", fileName = "Reaction_")]
public class MoodPreReaction : MoodReaction, IMoodReaction<ReactionInfo>, IMoodReaction<DamageInfo>
{
    public enum DamageStaminaCondition
    {
        Always,
        WhenCanPayFullStamina,
    }

    [Header("Conditions")]
    public bool ignoreDirection = false;
    public MoodSkill.DirectionFixer directionToWork = MoodSkill.DirectionFixer.LetAll;
    public bool canExecuteStunned;
    public MoodStance[] needStances;
    public MoodStance[] prohibitiveStances;

    [Header("Modifiers")]
    [FormerlySerializedAs("cost")]
    public float absoluteCost;
    public float multiplierDamageCost;
    public bool alwaysExecuteEvenWithoutStamina;

    [Space()]
    public float absoluteKnockback = 0;
    public float multiplierKnockbackByOrigin = 1f;
    public float multiplierKnockbackByDamage = 0f;
    public bool doKnockback;
    public AnimationCurve knockbackCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Space()]
    public DamageStaminaCondition whenModifyDamage;
    public ValueModifier damageModifier;
    public bool interruptCurrentSkill = true;


    [Header("Feedback")]
    public string animationTrigger;

    public ScriptableEvent[] events;

    public SoundEffect sfx;

    public override int Priority => base.Priority - 1;

    public virtual bool CanReact(ReactionInfo info, MoodPawn pawn)
    {
        Debug.LogFormat("[REACT] Can {0} react to {5} with {1}? Stunned:{2} && Stamina:{3} && Direction:{4}", pawn.name, name, 
            IsStunnedStatusValid(pawn), HasStamina(pawn, info.GetDamage(), alwaysExecuteEvenWithoutStamina), IsDirectionOK(pawn, info), info);
        return IsStunnedStatusValid(pawn) && HasStamina(pawn, info.GetDamage(), alwaysExecuteEvenWithoutStamina) && IsDirectionOK(pawn, info);
    }

    public virtual bool CanReact(DamageInfo info, MoodPawn pawn)
    {
        return CanReact((ReactionInfo)info, pawn);
    }

    private bool IsStunnedStatusValid(MoodPawn pawn)
    {
        if (canExecuteStunned) return true;
        else return !pawn.IsStunned(MoodPawn.StunType.Reaction);
    }

    private bool IsStanceStatusValid(MoodPawn pawn)
    {
        if (!pawn.HasAllStances(true, needStances)) return false;
        if (pawn.HasAnyStances(false, prohibitiveStances)) return false;
        return true;
    }

    public virtual void React(ref DamageInfo dmg, MoodPawn pawn)
    {
        ReactionInfo info = (ReactionInfo)dmg;
        switch (whenModifyDamage)
        {
            case DamageStaminaCondition.Always:
                damageModifier.Modify(ref dmg.damage, Mathf.FloorToInt);
                break;
            case DamageStaminaCondition.WhenCanPayFullStamina:
                if (HasStamina(pawn, dmg.damage))
                {
                    damageModifier.Modify(ref dmg.damage, Mathf.FloorToInt);
                }
                break;
            default:
                break;
        }
        DoKnockback(pawn, ref dmg.distanceKnockback, ref dmg.durationKnockback, dmg.damage);
        ReactAlways(ref info, pawn);
    }

    public virtual void React(ref ReactionInfo info, MoodPawn pawn)
    {
        DoKnockback(pawn, ref info.direction, ref info.duration, info.intensity);
        ReactAlways(ref info, pawn);
    }

    private void ReactAlways(ref ReactionInfo info, MoodPawn pawn)
    {
        pawn.DepleteStamina(GetStaminaCost(info.GetDamage()), MoodPawn.StaminaChangeOrigin.Reaction);
        if (interruptCurrentSkill) pawn.InterruptCurrentSkill();
        if (!string.IsNullOrEmpty(animationTrigger) && pawn.animator != null)
        {
            pawn.animator.SetTrigger(animationTrigger);
        }
        sfx.ExecuteIfNotNull(pawn.ObjectTransform);
        events.Invoke(pawn.ObjectTransform);
    }

    private void DoKnockback(MoodPawn pawn, ref Vector3 knockbackDistance, ref float knockbackDuration, float damageIntensity)
    {
        float knockbackMagnitude = knockbackDistance.magnitude;
        float newKnockbackMagnitude = (absoluteKnockback + damageIntensity * multiplierKnockbackByDamage + knockbackMagnitude * multiplierKnockbackByOrigin);
        if (knockbackDuration != 0f) knockbackDuration = knockbackDuration * newKnockbackMagnitude / knockbackMagnitude;
        knockbackDistance = knockbackDistance.normalized * knockbackMagnitude;
        if(doKnockback) pawn.Dash(knockbackDistance, knockbackDuration, knockbackCurve);
    }

    private Vector3 GetPosition(GameObject o)
    {
        MoodPawn enemy;
        if((enemy = o.GetComponentInParent<MoodPawn>()) != null)
        {
            return enemy.Position;
        }
        else return o.transform.position;
    }

    private bool HasStamina(MoodPawn pawn, int damage, bool ignore = false)
    {
        if (ignore) return true;
        return pawn.HasStamina(GetStaminaCost(damage));
    }

    private float GetStaminaCost(int damage)
    {
        return absoluteCost + multiplierDamageCost * damage;
    }

    private bool IsDirectionOK(MoodPawn pawn, ReactionInfo info)
    {
        if (ignoreDirection) return true;
        Vector3 attackDirection = info.origin != null? GetPosition(info.origin) - pawn.Position : info.direction;
        if (attackDirection != Vector3.zero)
        {
            float angleToSanitize = directionToWork.YAngleToSanitize(attackDirection, pawn.Direction);
            //Debug.LogFormat($"Is direction ok from {this.name}? Angle from {attackDirection} to {pawn.Direction} is {Vector3.SignedAngle(attackDirection, pawn.Direction, Vector3.up)} {angleToSanitize} -> {angleToSanitize == 0f}");
            return angleToSanitize == 0f;
        }
        else return true;
    }

}
