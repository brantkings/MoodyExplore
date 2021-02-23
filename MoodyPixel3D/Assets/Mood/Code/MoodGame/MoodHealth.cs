using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodHealth : Health, IMoodPawnBelonger
{
    MoodPawn pawn;

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

        if (damage.amount == 0) damage.shouldStaggerAnimation = false;

        return base.Damage(damage);
    }

    public MoodPawn GetMoodPawnOwner()
    {
        return pawn;
    }
}
