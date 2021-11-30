using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/Platformer", fileName = "Cond_Platformer")]
public class MoodPawnPlatformerCondition : MoodPawnCondition
{
    public enum State
    {
        ShouldBeTrue,
        ShouldBeFalse,
        Indifferent
    }

    public State grounded = State.Indifferent;
    public State walled = State.Indifferent;

    public override bool ConditionIsOK(MoodPawn pawn)
    {
        return ListConditions(pawn).All((x) => x);
    }

    public IEnumerable<bool> ListConditions(MoodPawn pawn)
    {
        yield return IsConditionOK(grounded, pawn.mover.Grounded);
        yield return IsConditionOK(walled, pawn.mover.Walled);
    }

    public bool IsConditionOK(State cond, bool what)
    {
        switch (cond)
        {
            case State.ShouldBeTrue:
                return what;
            case State.ShouldBeFalse:
                return !what;
            default:
                return true;
        }
    }

}
