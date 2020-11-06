using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPawnFinder
{
    MoodPawn GetPawn();
}

public class PawnPeekerControl : MonoBehaviour
{

    [Serializable]
    public class PeekerGetter : InterfaceGetter<IMoodPawnPeeker>
    {
        
    }


    public PeekerGetter getters;
    

    public void AddPawn(MoodPawn pawn)
    {
        foreach (IMoodPawnPeeker peeker in getters)
        {
            peeker.SetTarget(pawn);
        }
    }

    public void RemovePawn(MoodPawn pawn)
    {
        foreach (IMoodPawnPeeker peeker in getters)
        {
            peeker.UnsetTarget(pawn);
        }
    }
}
