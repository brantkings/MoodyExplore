using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Skill/Damaging Reaction", fileName = "Reaction_")]
public class MoodPreReaction_TakeDamage : MoodPreReaction
{
    [Header("Damage self")]
    public int damage = DamageInfo.BASE_SINGLE_UNIT_DAMAGE;
    public bool stagger = true;
    public bool ignorePhaseThrough = true;
    public float stunTime = 1f;

    public RelativeVector3 knockbackDirectionFromPawnDirection;
    public float knockbackDuration;

    public override void React(ref ReactionInfo info, MoodPawn pawn)
    {
        DamageInfo dmgInfo = new DamageInfo()
        {
            amount = damage,
            attackDirection = Vector3.zero,
            shouldStaggerAnimation = stagger,
            origin = pawn.gameObject,
            stunTime = this.stunTime,
            team = DamageTeam.Neutral,
            distanceKnockback = knockbackDirectionFromPawnDirection.Get(pawn.ObjectTransform),
            durationKnockback = knockbackDuration,
            unreactable = true,
            ignorePhaseThrough = true
        };

        Debug.LogWarningFormat("Gonna damage {0} by {1}", dmgInfo, name);

        if(Health.IsDamage(pawn.Damage(dmgInfo)))
        {
            base.React(ref info, pawn);
        }


    }
}
