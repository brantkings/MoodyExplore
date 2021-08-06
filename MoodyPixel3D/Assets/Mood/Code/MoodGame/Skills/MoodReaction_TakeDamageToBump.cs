using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects.Events;

[CreateAssetMenu(menuName = "Mood/Skill/Damaging Reaction", fileName = "Reaction_")]
public class MoodReaction_TakeDamageToBump : MoodReaction, IMoodReaction<ReactionInfo>
{
    [Header("Damage self")]
    public int damage = DamageInfo.BASE_SINGLE_UNIT_DAMAGE;
    public bool stagger = true;
    public bool ignorePhaseThrough = true;
    public TimeBeatManager.BeatQuantity stunTime = 8;
    public bool reactable;

    public RelativeVector3 knockbackDirectionFromPawnDirection;
    public float knockbackDuration;

    public ScriptableEvent[] events;

    public bool CanReact(ReactionInfo info, MoodPawn pawn)
    {
        return true;
    }

    public void React(ref ReactionInfo info, MoodPawn pawn)
    {
        DamageInfo dmgInfo = new DamageInfo()
        {
            damage = damage,
            attackDirection = Vector3.zero,
            shouldStaggerAnimation = stagger,
            origin = pawn.gameObject,
            stunTime = this.stunTime,
            team = DamageTeam.Neutral,
            distanceKnockback = knockbackDirectionFromPawnDirection.Get(pawn.ObjectTransform),
            durationKnockback = knockbackDuration,
            unreactable = !reactable,
            ignorePhaseThrough = true
        };

        events.Invoke(pawn.ObjectTransform);

        pawn.Damage(dmgInfo);
    }
}
