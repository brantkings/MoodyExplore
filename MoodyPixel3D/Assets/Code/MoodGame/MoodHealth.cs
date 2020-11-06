using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodHealth : Health
{
    MoodPawn pawn;

    private void Awake() {
        pawn = GetComponentInParent<MoodPawn>();
        if(pawn == null) Debug.LogWarningFormat("No pawn in {0}'s parent '{1}'", this, transform.root.name);  
    }

    public override bool Damage(DamageInfo damage)
    {
        if(pawn != null)
        {
            foreach(var react in pawn.GetActiveReactions())
            {
                if(react.CanReact(pawn))
                {
                    react.ReactToDamage(ref damage, pawn);
                }
            }
        }

        if(damage.amount != 0)
        {
            Debug.LogErrorFormat("Is going to damage {0}!", this);
            return base.Damage(damage);
        }
        else return false;
    }
}
