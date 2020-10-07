using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Skill/Change Stance")]
public class ChangeStanceMoodSkill : StaminaCostMoodSkill
{
    public MoodStance[] toAdd;
    public MoodStance[] toToggle;

    ///All mood skills consume can consume stances already, so this is duplicated.
    /// //public MoodStance[] toRemove;

    public bool ChangeStances(MoodPawn pawn)
    {
        bool changed = false;
        foreach(var stance in toAdd) changed |= pawn.AddStance(stance);
        //foreach(var stance in toRemove) changed |= pawn.RemoveStance(stance);
        return changed;
    }

    public override IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection)
    {
        ChangeStances(pawn);
        return base.Execute(pawn, skillDirection);
    }
}
