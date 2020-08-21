using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPawnPeeker : MonoBehaviour
{
    [Serializable]
    public class PeekerGetter : InterfaceGetter<IMoodPawnPeeker>
    {
        
    }


    public PeekerGetter getters;

    [SerializeField]
    [ReadOnly]
    private MoodPawn pawn;
    private void Awake()
    {
        pawn = MoodPlayerController.Instance.Pawn;
    }

    private void OnEnable()
    {
        if (pawn != null)
        {
            foreach (IMoodPawnPeeker peeker in getters)
            {
                peeker.SetTarget(pawn);
            }
        }
    }
    
    private void OnDisable()
    {
        if (pawn != null)
        {
            foreach (IMoodPawnPeeker peeker in getters)
            {
                peeker.UnsetTarget(pawn);
            }
        }
    }
}
