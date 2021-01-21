using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPawnGetter : PawnGetter
{
    [SerializeField]
    [ReadOnly]
    private MoodPawn pawn;
    private void Awake()
    {
        pawn = MoodPlayerController.Instance.Pawn;
    }

    public override MoodPawn GetPawn()
    {
        return pawn;
    }
}
