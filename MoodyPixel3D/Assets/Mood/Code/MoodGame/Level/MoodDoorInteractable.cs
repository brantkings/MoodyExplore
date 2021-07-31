using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoodDoor))]
public class MoodDoorInteractable : MoodInteractable
{

    MoodDoor door;
    Coroutine routine;
    bool isOpeningDoor;
    private void Awake()
    {
        door = GetComponent<MoodDoor>();
    }

    public override void Interact(MoodInteractor interactor)
    {
        MoodPawn pawn = interactor.GetComponentInParent<MoodPawn>();
        if(pawn != null)
        {
            routine = StartCoroutine(DoorRoutine(pawn));
        }
    }

    private IEnumerator DoorRoutine(MoodPawn pawn)
    {
        isOpeningDoor = true;
        yield return door.OpenDoorRoutine(pawn);
        isOpeningDoor = false;
    }

    public override bool IsBeingInteracted()
    {
        return isOpeningDoor;
    }

}
