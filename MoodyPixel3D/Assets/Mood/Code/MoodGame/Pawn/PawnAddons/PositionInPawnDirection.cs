using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInPawnDirection : MonoBehaviour
{
    public enum PawnDirection
    {
        Velocity,
        Direction,
        LatestMovement,
    }

    public PawnDirection directionToGet;
    public bool normalized = true;
    public float multiplier = 1f;
    public bool alsoDirectForwardToDirection;

    private Vector3 distanceY;

    [SerializeField]
    private MoodPawn pawn;

    private void Awake()
    {
        if (pawn == null) pawn = GetComponentInParent<MoodPawn>();
        distanceY = Vector3.up * (transform.position - pawn.Position).y;
    }

    private Vector3 GetDirection(PawnDirection d)
    {
        switch (d)
        {
            case PawnDirection.Velocity:
                return pawn.Velocity;
            case PawnDirection.Direction:
                return pawn.Direction;
            case PawnDirection.LatestMovement:
                return pawn.MovingDirection;
            default:
                return Vector3.zero;
        }
    }

    public void Update()
    {
        Vector3 dir = GetDirection(directionToGet);
        if (normalized) dir.Normalize();
        transform.position = pawn.Position + dir * multiplier + distanceY;
        if (alsoDirectForwardToDirection) transform.forward = dir;
    }
}
