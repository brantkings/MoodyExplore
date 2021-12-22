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
            Debug.LogFormat(this, "[PAWN PEEKER] {0} have {1}", pawn, peeker);
            peeker.SetTarget(pawn);
        }
    }

    public void RemovePawn(MoodPawn pawn)
    {
        foreach (IMoodPawnPeeker peeker in getters)
        {
            Debug.LogFormat(this, "[PAWN PEEKER] {0} lost {1}", pawn, peeker);
            peeker.UnsetTarget(pawn);
        }
    }
}
