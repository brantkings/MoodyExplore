using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Skill/Change Stance")]
public class ChangeStanceMoodSkill : StaminaCostMoodSkill
{
    [Header("Stance change")]
    public float stanceChangeTime = 0f;
    public ActivateableMoodStance[] toAdd;
    public ActivateableMoodStance[] toToggle;

    ///All mood skills consume can consume stances already, so this is duplicated.
    /// //public MoodStance[] toRemove;

    public bool ChangeStances(MoodPawn pawn)
    {
        bool changed = false;
        foreach(var stance in toAdd) changed |= WhatIsResult(pawn.AddStance(stance));
        foreach(var stance in toToggle) changed |= WhatIsResult(pawn.ToggleStance(stance));
        //foreach(var stance in toRemove) changed |= pawn.RemoveStance(stance);
        return changed;
    }

    private bool WhatIsResult(MoodPawn.StanceModificationResult result)
    {
        switch (result)
        {
            case MoodPawn.StanceModificationResult.No:
                return false;
            case MoodPawn.StanceModificationResult.Postponed:
                return true;
            case MoodPawn.StanceModificationResult.Yes:
                return true;
            default:
                return false;
        }
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        float cost = 0f;

        if(ChangeStances(pawn)) 
            cost = stanceChangeTime;

        return base.ExecuteEffect(pawn, skillDirection) + cost;
    }

    public override IEnumerable<MoodStance> GetStancesThatWillBeAdded()
    {
        foreach (MoodStance stance in toAdd) 
            yield return stance;
        foreach (MoodStance stance in toToggle) 
            yield return stance;
    }
}
