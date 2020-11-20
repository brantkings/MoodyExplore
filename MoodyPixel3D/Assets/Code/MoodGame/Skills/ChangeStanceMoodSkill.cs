using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Skill/Change Stance")]
public class ChangeStanceMoodSkill : StaminaCostMoodSkill
{
    [Header("Stance change")]
    public float stanceChangeTime = 0f;
    public MoodStance[] toAdd;
    public MoodStance[] toToggle;

    ///All mood skills consume can consume stances already, so this is duplicated.
    /// //public MoodStance[] toRemove;

    public bool ChangeStances(MoodPawn pawn)
    {
        bool changed = false;
        foreach(var stance in toAdd) changed |= pawn.AddStance(stance);
        foreach(var stance in toToggle) changed |= pawn.ToggleStance(stance);
        //foreach(var stance in toRemove) changed |= pawn.RemoveStance(stance);
        return changed;
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
