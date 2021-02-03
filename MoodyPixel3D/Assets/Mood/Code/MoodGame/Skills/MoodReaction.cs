using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Mood/Skill/General Reaction", fileName = "Reaction_")]
public class MoodReaction : ScriptableObject
{
    public enum ActionType
    {
        Damage,
        Bump,
        Bumped,
        All
    }

    public enum DamageStaminaCondition
    {
        Always,
        WhenCanPayFullStamina,
    }

    [Header("Conditions")]
    public ActionType origin = ActionType.All;
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

    [Space()]
    public DamageStaminaCondition whenModifyDamage;
    public ValueModifier damageModifier;
    public bool interruptCurrentSkill = true;


    [Header("Feedback")]
    public string animationTrigger;

    public ScriptableEvent[] events;

    public SoundEffect sfx;

    public struct ReactionInfo
    {
        public GameObject origin;
        public Vector3 direction;
        public Vector3 normal;
        public float knockbackDuration;
        public int intensity;

        public static explicit operator ReactionInfo(DamageInfo info)
        {
            return new ReactionInfo()
            {
                origin = info.origin,
                intensity = info.amount,
                direction = info.distanceKnockback,
                knockbackDuration = info.durationKnockback
            };
        }

        public ReactionInfo SetNormal(Vector3 normal)
        {
            this.normal = normal;
            return this;
        }

        public ReactionInfo(GameObject origin, Vector3 knockback, float duration)
        {
            this.origin = origin;
            this.intensity = 0;
            this.knockbackDuration = duration;
            this.direction = knockback;

            this.normal = Vector3.zero;
        }

        public int GetDamage()
        {
            return intensity;
        }

        public override string ToString()
        {
            return string.Format("[{0} by {1}, {2} intensity, {3} duration]", direction, origin?.name, intensity, knockbackDuration);
        }

    }

    public virtual bool CanReact(MoodPawn pawn, DamageInfo info)
    {
        return CanReact(pawn, (ReactionInfo) info, ActionType.Damage);
    }

    public virtual bool CanReact(MoodPawn pawn, ReactionInfo info, ActionType type)
    {
        Debug.LogWarningFormat("Can {0} react with {1}? Origin:{2} && Stunned:{3} && Stamina:{4} && Direction:{5}", pawn.name, name, 
            IsValidTypeForThis(type), IsStunnedStatusValid(pawn), HasStamina(pawn, info.GetDamage(), alwaysExecuteEvenWithoutStamina), IsDirectionOK(pawn, info));
        return IsValidTypeForThis(type) && IsStunnedStatusValid(pawn) && HasStamina(pawn, info.GetDamage(), alwaysExecuteEvenWithoutStamina) && IsDirectionOK(pawn, info);
    }

    protected virtual bool IsValidTypeForThis(ActionType type)
    {
        if (origin == ActionType.All) return true;
        else return type == origin;
    }

    private bool IsStunnedStatusValid(MoodPawn pawn)
    {
        if (canExecuteStunned) return true;
        else return !pawn.IsStunned();
    }

    private bool IsStanceStatusValid(MoodPawn pawn)
    {
        if (!pawn.HasAllStances(true, needStances)) return false;
        if (pawn.HasAnyStances(false, prohibitiveStances)) return false;
        return true;

    }

    public virtual void ReactToDamage(ref DamageInfo dmg, MoodPawn pawn)
    {
        ReactionInfo info = (ReactionInfo)dmg;
        switch (whenModifyDamage)
        {
            case DamageStaminaCondition.Always:
                damageModifier.Modify(ref dmg.amount, Mathf.FloorToInt);
                break;
            case DamageStaminaCondition.WhenCanPayFullStamina:
                if(HasStamina(pawn, dmg.amount))
                {
                    damageModifier.Modify(ref dmg.amount, Mathf.FloorToInt);
                }
                break;
            default:
                break;
        }
        ChangeKnockback(ref dmg.distanceKnockback, ref dmg.durationKnockback, dmg.amount);
        React(info, pawn);
    }

    public virtual void ReactToBump(ref ReactionInfo info, MoodPawn pawn)
    {
        ChangeKnockback(ref info.direction, ref info.knockbackDuration, info.intensity);
        React(info, pawn);
    }

    protected virtual void React(ReactionInfo info, MoodPawn pawn)
    {
        Debug.LogFormat("{0} is reacting with {2} to {1}", pawn.name, info, name);
        pawn.DepleteStamina(GetStaminaCost(info.GetDamage()), MoodPawn.StaminaChangeOrigin.Reaction);
        if (interruptCurrentSkill) pawn.InterruptCurrentSkill();
        if (!string.IsNullOrEmpty(animationTrigger))
        {
            pawn.animator.SetTrigger(animationTrigger);
        }
        sfx.ExecuteIfNotNull(pawn.ObjectTransform);
        events.Invoke(pawn.ObjectTransform);
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
