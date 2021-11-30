using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_SelfHC_", menuName = "Mood/Skill/Self Health Change", order = 0)]
public class DamageSelfDelaySkill : CostStaminaDelaySkill
{
    [Header("Health change")]
    public int damage = DamageInfo.BASE_SINGLE_UNIT_DAMAGE;
    public float stunTime = 0f;

    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.Damage(GetDamageInfo(pawn));
        return base.ExecuteEffect(pawn, skillDirection);
    }

    private DamageInfo GetDamageInfo(MoodPawn pawn)
    {
        return new DamageInfo()
        {
            damage = damage,
            feedbacks = true,
            freezeFrame = new DamageInfo.FreezeFrameData()
            {
                freezeFrameAdd = 0,
                freezeFrameDelay = 0,
                freezeFrameDelayRealTime = 0,
                freezeFrameEase = DG.Tweening.Ease.Linear,
                freezeFrameMult = 0,
                freezeFrameTweenDurationRatio = 0,
            },
            origin = pawn.gameObject,
            stunTime = stunTime,
            unreactable = true

        };
    }
}
