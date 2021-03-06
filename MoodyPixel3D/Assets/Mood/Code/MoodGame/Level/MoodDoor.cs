using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

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
    public LHH.Switchable.Switchable openedSwitchable;
    public TransformGetter eventTransform;
    public ScriptableEvent[] onTouchDoor;
    public ScriptableEvent[] onAnimDoorOpen;
    public ScriptableEvent[] onAnimDoorMiddle;
    public ScriptableEvent[] onAnimDoorClose;
    public ScriptableEvent[] onDoorEnd;

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
        onTouchDoor.Invoke(eventTransform.Get(transform));

        Level.GetRoom(positionEntry)?.Deactivate();
        yield return MoveTo(opener, positionEntry, positionExit - positionEntry, initialSlideDuration, 0.5f);
        Level.GetRoom(positionExit)?.Activate();

        OnStartAnimationOpen?.Invoke(opener);
        onAnimDoorOpen.Invoke(eventTransform.Get(transform));
        yield return AnimateOpenDoor();
        onAnimDoorMiddle.Invoke(eventTransform.Get(transform));
        OnEndAnimationOpen?.Invoke(opener);
        yield return MoveTo(opener, positionExit, (positionExit - positionEntry).normalized, walkAnimationDuration, 0.5f);
        OnStartAnimationClose?.Invoke(opener);
        onAnimDoorClose.Invoke(eventTransform.Get(transform));
        yield return AnimateCloseDoor();
        Unlock(opener);
        OnEndOpen?.Invoke(opener);
        onDoorEnd.Invoke(eventTransform.Get(transform));
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


    private IEnumerator MoveTo(MoodPawn pawn, Vector3 position, Vector3 direction, float duration, float durationAfter)
    {
        pawn.RotateDash(Vector3.SignedAngle(pawn.Direction, direction, Vector3.up), duration);
        yield return pawn.mover.TweenMoverPosition(position - pawn.mover.Position, duration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(durationAfter);
    }

    private IEnumerator AnimateOpenDoor()
    {
        openedSwitchable.Set(true);
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator AnimateCloseDoor()
    {
        openedSwitchable.Set(false);
        yield return new WaitForSeconds(0.25f);
    }

}
