using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoodDoor : MonoBehaviour
{
    public delegate void DelDoorEvent(MoodPawn opener);
    public event DelDoorEvent OnStartOpen;
    public event DelDoorEvent OnStartAnimationOpen;
    public event DelDoorEvent OnEndAnimationOpen;
    public event DelDoorEvent OnStartAnimationClose;
    public event DelDoorEvent OnEndOpen;

    [Header("Door properties")]
    public LayerMask groundMask;
    public float distanceFromDoor = 1f;
    public float initialSlideDuration = 0.1f;
    public float walkAnimationDuration = 1f;
    public float doorAnimationDuration = 0.25f;

    private MoodLevel _level;
    private MoodLevel Level
    {
        get
        {
            if(_level == null) _level = GetComponentInParent<MoodLevel>();
            return _level;
        }
    }

    private void OnDrawGizmosSelected()
    {
        GetDoorSpawnPositions(out Vector3 positionA, out Vector3 positionB);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(positionA, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(positionB, 0.5f);
    }

    private void GetDoorSpawnPositions(out Vector3 positionA, out Vector3 positionB)
    {
        positionA = transform.position + transform.forward * distanceFromDoor;
        positionB = transform.position - transform.forward * distanceFromDoor;
    }


    public IEnumerator OpenDoorRoutine(MoodPawn opener)
    {
        Vector3 positionA, positionB, positionEntry, positionExit;
        GetDoorSpawnPositions(out positionA, out positionB);
        GetRoomPositions(opener.Position, positionA, positionB, out positionExit, out positionEntry);

        OnStartOpen?.Invoke(opener);
        Lock(opener);

        Level.GetRoom(positionEntry)?.Deactivate();
        yield return MoveTo(opener, positionEntry, -transform.forward, initialSlideDuration);
        Level.GetRoom(positionExit)?.Activate();

        OnStartAnimationOpen?.Invoke(opener);
        yield return AnimateOpenDoor();
        OnEndAnimationOpen?.Invoke(opener);
        yield return MoveTo(opener, positionExit, (positionExit - positionEntry).normalized, walkAnimationDuration);
        OnStartAnimationClose?.Invoke(opener);
        yield return AnimateCloseDoor();
        Unlock(opener);
        OnEndOpen?.Invoke(opener);
    }

    private void Lock(MoodPawn pawn)
    {
        if (pawn != null)
        {
            pawn.AddStunLock(MoodPawn.StunType.Action, "Door");
            pawn.AddStunLock(MoodPawn.StunType.Movement, "Door");
            pawn.AddStunLock(MoodPawn.StunType.Reaction, "Door");
        }
    }

    private void Unlock(MoodPawn pawn)
    {
        if (pawn != null)
        {
            pawn.RemoveStunLock(MoodPawn.StunType.Action, "Door");
            pawn.RemoveStunLock(MoodPawn.StunType.Movement, "Door");
            pawn.RemoveStunLock(MoodPawn.StunType.Reaction, "Door");
        }
    }

    private void GetRoomPositions(in Vector3 mainPos, in Vector3 positionA, in Vector3 positionB, out Vector3 farthestPosition, out Vector3 closestPosition)
    {
        if (Vector3.Distance(positionA, mainPos) < Vector3.Distance(positionB, mainPos))
        {
            closestPosition = positionA; farthestPosition = positionB;
        }
        else
        {
            closestPosition = positionB; farthestPosition = positionA;
        }
    }


    private IEnumerator MoveTo(MoodPawn pawn, Vector3 position, Vector3 direction, float duration)
    {
        pawn.ObjectTransform.position = position;
        pawn.ObjectTransform.forward = direction;
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator AnimateOpenDoor()
    {
        GetComponentInChildren<Renderer>().enabled = false;
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator AnimateCloseDoor()
    {
        GetComponentInChildren<Renderer>().enabled = true;
        yield return new WaitForSeconds(0.25f);
    }

}
