using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Mood/Skill/Reaction/Basic Bump on Wall", fileName = "Reaction_Bump_")]
public class MoodReactionBasicBump : MoodReaction, IMoodReaction<ReactionInfo>
{
    public bool interruptCurrentSkill;
    public float stunDurationMultiplier = 1f;
    public float dashMultiplier = 1f;
    public Ease dashEase = Ease.OutSine;

    [Space()]
    public float animationParameterMultiplier = 0.5f;

    public virtual bool CanReact(ReactionInfo info, MoodPawn pawn)
    {
        return true;
    }

    public virtual void React(ref ReactionInfo info, MoodPawn pawn)
    {
        pawn.AddStunLockTimer(MoodPawn.StunType.Action, name, info.duration * stunDurationMultiplier);
        pawn.SetDamageAnimationTween(info.direction.normalized * animationParameterMultiplier, info.duration * stunDurationMultiplier, 0f);
        pawn.Dash(info.direction * dashMultiplier, info.duration * dashMultiplier, dashEase);
        if(interruptCurrentSkill) pawn.InterruptCurrentSkill();
    }
}
