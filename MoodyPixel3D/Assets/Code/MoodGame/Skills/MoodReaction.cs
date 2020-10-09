using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Skill/Damage Reaction", fileName = "Reaction_")]
public class MoodReaction : ScriptableObject
{
    public float cost;

    public ValueModifier damageModifier;

    public string animationTrigger;

    public virtual bool CanReact(MoodPawn pawn)
    {
        return pawn.HasStamina(cost);
    }

    public virtual void ReactToDamage(ref int damage, GameObject origin, MoodPawn pawn)
    {
        pawn.DepleteStamina(cost);
        damageModifier.Modify(ref damage, Mathf.FloorToInt);
        if(!string.IsNullOrEmpty(animationTrigger))
        {
            pawn.animator.SetTrigger(animationTrigger);
        }
    }
}
