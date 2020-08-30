using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnGetter : MonoBehaviour
{
    [Serializable]
    public class PeekerGetter : InterfaceGetter<IMoodPawnPeeker>
    {
        
    }


    public PeekerGetter getters;
    
    protected abstract MoodPawn GetPawn();

    private void OnEnable()
    {
        MoodPawn pawn = GetPawn();
        if (pawn != null)
        {
            AddPawn(pawn);
        }
    }
    
    private void OnDisable()
    {
        MoodPawn pawn = GetPawn();
        if (pawn != null)
        {
            RemovePawn(pawn);
        }
    }

    protected void AddPawn(MoodPawn pawn)
    {
        foreach (IMoodPawnPeeker peeker in getters)
        {
            peeker.SetTarget(pawn);
        }
    }

    protected void RemovePawn(MoodPawn pawn)
    {
        foreach (IMoodPawnPeeker peeker in getters)
        {
            peeker.UnsetTarget(pawn);
        }
    }
}
