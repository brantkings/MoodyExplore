using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodExperienceGiver : MonoBehaviour
{
    public int amountXP = 1;

    MoodPawn _pawn;


    private void Awake()
    {
        _pawn = GetComponentInParent<MoodPawn>();

        if (_pawn == null)
        {
            Debug.LogError("No pawn to die to in {0}!", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        _pawn.OnPawnDeath += OnPawnDeath;
    }

    private void OnDisable()
    {
        _pawn.OnPawnDeath -= OnPawnDeath;
    }

    private void OnPawnDeath(MoodPawn pawn, DamageInfo info)
    {
        if(CanGiveExperienceTo(info.origin.GetComponentInParent<MoodPawn>(), out MoodExperiencer experiencer))
        {
            Debug.LogFormat("[EXP] {0} giving {1} {2} experience.", this, experiencer, amountXP);
            experiencer.GetXP(this, amountXP);
        }
    }

    private bool CanGiveExperienceTo(MoodPawn origin, out MoodExperiencer experiencer)
    {
        experiencer = null;
        if (amountXP != 0)
        {
            experiencer = origin?.GetComponentInChildren<MoodExperiencer>();
            return experiencer != null;
        }
        else return false;
    }
}
