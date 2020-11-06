using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnGetter : MonoBehaviour, IPawnFinder
{
    public PawnPeekerControl control;

    private void Reset()
    {
        control = GetComponentInChildren<PawnPeekerControl>();
    }

    private void OnEnable()
    {
        if(control != null)
        {
            control.AddPawn(GetPawn());
        }
    }

    private void OnDisable()
    {
        if(control != null)
        {
            control.RemovePawn(GetPawn());
        }
    }

    public abstract MoodPawn GetPawn();
}
