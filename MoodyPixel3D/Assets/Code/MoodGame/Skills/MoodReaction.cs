using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Mood/Skill/Damage Reaction", fileName = "Reaction_")]
public class MoodReaction : ScriptableObject
{
    [FormerlySerializedAs("cost")]
    public float absoluteCost;
    public float multiplierDamageCost;

    public MoodSkill.DirectionFixer directionToWork;

    public ValueModifier damageModifier;

    public string animationTrigger;

    public SoundEffect sfx;

    private bool IsDirectionOK(MoodPawn pawn, DamageInfo info)
    {
        Vector3 attackDirection = info.attackDirection;
        if (info.attackDirection != Vector3.zero)
        {
            return directionToWork.YAngleToSanitize(info.attackDirection, pawn.Direction) == 0f;
        }
        else return true;
    }

    public virtual bool CanReact(MoodPawn pawn, DamageInfo info)
    {
        return pawn.HasStamina(absoluteCost);
    }

    public virtual void ReactToDamage(ref DamageInfo dmg, MoodPawn pawn)
    {
        pawn.DepleteStamina(absoluteCost + dmg.amount * multiplierDamageCost);
        damageModifier.Modify(ref dmg.amount, Mathf.FloorToInt);
        if(!string.IsNullOrEmpty(animationTrigger))
        {
            pawn.animator.SetTrigger(animationTrigger);
        }
        sfx.ExecuteIfNotNull(pawn.ObjectTransform);
    }
}
