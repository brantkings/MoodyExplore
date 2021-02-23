using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoodReactionUseSkill : MoodReaction, IMoodReaction<ReactionInfo>
{
    public MoodSkill skill;
    public MoodSkill.DirectionFixer directionFixer;
    public bool onlyReactIfCanUseSkill;
    public float delay;

    public bool CanReact(ReactionInfo info, MoodPawn pawn)
    {
        if (onlyReactIfCanUseSkill) return pawn.CanUseSkill(skill);
        return true;
    }

    public void React(ref ReactionInfo info, MoodPawn pawn)
    {
        Vector3 direction = GetDirection(info, pawn);
        TweenCallback act = () =>
        {
            pawn.ExecuteSkill(skill, direction);
        };
        if(delay > 0f)
        {
            pawn.DelayedAction(act, delay);
        }
        else
        {
            act();
        }
    }

    public void ExecuteSkill()
    { 
    }

    private Vector3 GetDirection(ReactionInfo info, MoodPawn pawn)
    {
        Vector3 direction = info.direction;
        return directionFixer.Sanitize(direction, pawn.Direction);
    }
}
