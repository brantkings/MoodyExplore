using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPawnGetter : PawnGetter
{
    public MoodPawn pawn;
    
    protected override MoodPawn GetPawn()
    {
        return pawn;
    }
}
