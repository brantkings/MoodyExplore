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

    public override bool Damage(int amount, DamageTeam team, GameObject origin = null)
    {
        if(pawn != null)
        {
            foreach(var react in pawn.GetActiveReactions())
            {
                if(react.CanReact(pawn))
                {
                    react.ReactToDamage(ref amount, origin, pawn);
                }
            }
        }

        if(amount != 0)
        {
            return base.Damage(amount, team, origin);
        }
        else return false;
    }
}
