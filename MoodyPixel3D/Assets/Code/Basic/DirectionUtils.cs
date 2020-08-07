using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    Forward,
    Back,
    Right,
    Left,
    Up,
    Down
}


[System.Serializable]
public struct DirectionGetter
{
    public Transform GetTransformOrigin(Transform myself)
    {
        if (customFrom != null) return customFrom;
        else return myself;   
    }

    public Vector3 Get(Transform defaultTransform)
    {
        return DirectionUtils.GetDirectionFrom(direction, GetTransformOrigin(defaultTransform));
    }

    public Direction direction;
    public Transform customFrom;
}


public static class DirectionUtils
{
    public static Vector3 GetDirectionFrom(Direction dir, Transform from)
    {
        switch (dir)
        {
            case Direction.Forward:
                return from.forward;
            case Direction.Back:
                return -from.forward;
            case Direction.Right:
                return from.right;
            case Direction.Left:
                return -from.right;
            case Direction.Up:
                return from.up;
            case Direction.Down:
                return -from.up;
            default:
                return from.forward;
        }
    }
}
