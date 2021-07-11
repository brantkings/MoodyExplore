using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodHealth : Health, IMoodPawnBelonger
{
    MoodPawn pawn;
    private MoodParameter<int> _maxHealthParameter;


    public override int MaxLife
    {
        get
        {
            if (_maxHealthParameter == null)
            {
                _maxHealthParameter = base.MaxLife;
                _maxHealthParameter.OnChange += MaxHealthParameterChange;
            }
            return _maxHealthParameter;
        }
    }

    private DamageInfo MakeSelfLifeChange(int amount)
    {
        return new DamageInfo()
        {
            damage = amount,
            stunTime = 0f,
            team = DamageTeam.Neutral,
            unreactable = true,
        };
    }

    private void MaxHealthParameterChange(int before, int after)
    {
        CallMaxHealthChange();
    }

    public MoodParameter<int> GetMaxHealthParameter()
    {
        return _maxHealthParameter;
    }

    private void Awake() {
        pawn = GetComponentInParent<MoodPawn>();
        if(pawn == null) Debug.LogWarningFormat("No pawn in {0}'s parent '{1}'", this, transform.root.name);  
    }

    public override DamageResult Damage(DamageInfo damage)
    {
        if(CanDamage(damage) && pawn != null)
        {
            if(!damage.unreactable)
            {
                foreach(var react in pawn.GetActiveReactions<DamageInfo>())
                {
                    if(react.CanReact(damage, pawn))
                    {
                        Debug.LogFormat("{0} reacting with '{1}' to damage {2}", pawn.name, react, damage);
                        react.React(ref damage, pawn);
                    }
                }
            }
        }

        if (damage.damage == 0) damage.shouldStaggerAnimation = false;

        return base.Damage(damage);
    }

    public MoodPawn GetMoodPawnOwner()
    {
        return pawn;
    }
}
