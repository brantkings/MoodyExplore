using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Mood/Skill/Damage Reaction", fileName = "Reaction_")]
public class MoodReaction : ScriptableObject
{
    public enum ActionType
    {
        Damage,
        Bump,
        All
    }

    [Header("Conditions")]
    public ActionType origin = ActionType.All;
    public MoodSkill.DirectionFixer directionToWork;

    [Header("Modifiers")]
    [FormerlySerializedAs("cost")]
    public float absoluteCost;
    public float multiplierDamageCost;

    [Space()]
    public float absoluteKnockback = 0;
    public float multiplierKnockbackByOrigin = 1f;
    public float multiplierKnockbackByDamage = 0f;

    [Space()]
    public ValueModifier damageModifier;


    [Header("Feedback")]
    public string animationTrigger;

    public SoundEffect sfx;

    public struct ReactionInfo
    {
        public GameObject origin;
        public Vector3 direction;
        public Vector3 knockback;
        public float knockbackDuration;
        public int intensity;

        public static explicit operator ReactionInfo(DamageInfo info)
        {
            return new ReactionInfo()
            {
                origin = info.origin,
                intensity = info.amount,
                direction = info.attackDirection,
                knockback = info.distanceKnockback,
                knockbackDuration = info.durationKnockback
            };
        }

    }

    public virtual bool CanReact(MoodPawn pawn, DamageInfo info)
    {
        return CanReact(pawn, (ReactionInfo) info, ActionType.Damage);
    }

    public virtual bool CanReact(MoodPawn pawn, ReactionInfo info, ActionType type)
    {
        return IsValidOriginForThis(type) && pawn.HasStamina(absoluteCost) && IsDirectionOK(pawn, info);
    }

    private bool IsValidOriginForThis(ActionType type)
    {
        if (origin == ActionType.All) return true;
        else return type == origin;
    }

    public virtual void ReactToDamage(ref DamageInfo dmg, MoodPawn pawn)
    {
        ReactionInfo info = (ReactionInfo)dmg;
        info.intensity /= 10;
        damageModifier.Modify(ref dmg.amount, Mathf.FloorToInt);
        ChangeKnockback(ref dmg.distanceKnockback, ref dmg.durationKnockback, dmg.amount);
        React(info, pawn);
    }

    public virtual void ReactToBump(ref ReactionInfo info, MoodPawn pawn)
    {
        ChangeKnockback(ref info.knockback, ref info.knockbackDuration, info.intensity);
        React(info, pawn);
    }

    protected virtual void React(ReactionInfo info, MoodPawn pawn)
    {
        Debug.LogWarningFormat("Depleting {0} + {1} * {2}", absoluteCost, info.intensity, multiplierDamageCost);
        pawn.DepleteStamina(absoluteCost + info.intensity * multiplierDamageCost, MoodPawn.StaminaChangeOrigin.Reaction);
        if (!string.IsNullOrEmpty(animationTrigger))
        {
            pawn.animator.SetTrigger(animationTrigger);
        }
        sfx.ExecuteIfNotNull(pawn.ObjectTransform);
    }

    private void ChangeKnockback(ref Vector3 knockbackDistance, ref float knockbackDuration, float damageIntensity)
    {
        float knockbackMagnitude = knockbackDistance.magnitude;
        float newKnockbackMagnitude = (absoluteKnockback + damageIntensity * multiplierKnockbackByDamage + knockbackMagnitude * multiplierKnockbackByOrigin);
        if (knockbackDuration != 0f) knockbackDuration = knockbackDuration * newKnockbackMagnitude / knockbackMagnitude;
        knockbackDistance = knockbackDistance.normalized * knockbackMagnitude;
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

    private bool IsDirectionOK(MoodPawn pawn, ReactionInfo info)
    {
        Vector3 attackDirection = GetPosition(info.origin) - pawn.Position;
        if (attackDirection != Vector3.zero)
        {
            float angleToSanitize = directionToWork.YAngleToSanitize(attackDirection, pawn.Direction);
            //Debug.LogFormat($"Is direction ok from {this.name}? Angle from {attackDirection} to {pawn.Direction} is {Vector3.SignedAngle(attackDirection, pawn.Direction, Vector3.up)} {angleToSanitize} -> {angleToSanitize == 0f}");
            return angleToSanitize == 0f;
        }
        else return true;
    }
}
