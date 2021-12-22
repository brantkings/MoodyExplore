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
    public float dashAbsoluteAdd = 0f;
    public bool distanceInBeats = true;
    public bool bumpeable = false;
    public Ease dashEase = Ease.OutSine;

    [Space()]
    public float animationParameterMultiplier = 0.5f;

    public virtual bool CanReact(ReactionInfo info, MoodPawn pawn)
    {
        return true;
    }

    public virtual void React(ref ReactionInfo info, MoodPawn pawn)
    {
        pawn.AddStunLockTimer(MoodPawn.LockType.Action, name, info.duration * stunDurationMultiplier);
        pawn.SetDamageAnimationTween(info.direction.normalized * animationParameterMultiplier, info.duration * stunDurationMultiplier, 0f);
        pawn.Dash(dashMultiplier * info.direction + dashAbsoluteAdd * info.direction.normalized, distanceInBeats, info.duration * dashMultiplier, bumpeable, dashEase);
        if(interruptCurrentSkill) pawn.InterruptCurrentSkill();
    }
}
