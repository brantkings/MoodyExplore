using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_RotateToTarget_Core : MonoBehaviour
{
    public enum Direction
    {
        Forward,
        Up,
        Right
    }

    public Detector detector;
    public Transform toDirect;
    public Direction direction = Direction.Forward;

    [SerializeField]
    private bool onXZPlane = true;

    [Space()]
    public bool debug;
    protected virtual void SetDirection(Transform who, Vector3 dir)
    {
        if (onXZPlane) dir.y = 0f;
        switch (direction)
        {
            case Direction.Forward:
                who.forward = dir;
                break;
            case Direction.Up:
                who.up = dir;
                break;
            case Direction.Right:
                who.right = dir;
                break;
            default:
                break;
        }
        if (debug) Debug.LogFormat("Setting direction to {0}, {1}", dir, transform.forward);
    }


    protected void RotateIfDetecting()
    {
        if (debug) Debug.LogFormat("{0} is detecting {1} and distance is {2}.", transform.parent, detector != null ? detector.IsDetecting : false, detector != null ? detector.GetDistanceToTarget() : Vector3.zero);

        if (detector != null && detector.IsDetecting)
        {
            Vector3? distance = detector.GetDistanceToTarget();
            if (distance.HasValue)
                SetDirection(toDirect ?? transform, distance.Value);
        }
    }
}
