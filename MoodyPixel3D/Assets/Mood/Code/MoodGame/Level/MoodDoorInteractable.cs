using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoodDoor))]
public class MoodDoorInteractable : MoodInteractable
{

    MoodDoor door;
    private void Awake()
    {
        door = GetComponent<MoodDoor>();
    }

    public override void Interact(MoodInteractor interactor)
    {
        MoodPawn pawn = interactor.GetComponentInParent<MoodPawn>();
        if(pawn != null)
        {
            StartCoroutine(door.OpenDoorRoutine(pawn));
        }
    }

}
