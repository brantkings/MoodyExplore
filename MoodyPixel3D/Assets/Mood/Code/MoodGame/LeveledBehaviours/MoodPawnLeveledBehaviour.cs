using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodPawnLeveledBehaviour : LHH.LeveledBehaviours.LeveledBehaviour
{
    private MoodPawn _pawn;

    protected virtual MoodPawn Pawn
    {
        get
        {
            return _pawn;
        }
    }

    protected virtual void Awake()
    {
        _pawn = GetComponentInParent<MoodPawn>();
        Initiate(_pawn);
    }

    protected abstract void Initiate(MoodPawn pawn);

    protected override void ApplyLevel(float level)
    {
        ApplyLevel(Pawn, level);
    }

    abstract protected void ApplyLevel(MoodPawn pawn, float level);
}
