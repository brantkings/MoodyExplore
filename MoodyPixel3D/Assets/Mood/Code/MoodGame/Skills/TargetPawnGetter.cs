using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPawnGetter : PawnGetter, IMoodPawnSetter
{
    public MoodPawn pawn;

    public MoodPawn GetMoodPawnOwner()
    {
        return GetPawn();
    }

    public override MoodPawn GetPawn()
    {
        return pawn;
    }

    public void SetMoodPawnOwner(MoodPawn pawn)
    {
        this.pawn = pawn;
    }
}
