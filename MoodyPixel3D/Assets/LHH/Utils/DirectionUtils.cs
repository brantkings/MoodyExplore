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
public struct DirectionManipulator
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

    public void Set(Transform defaultTransform, Vector3 value)
    {
        DirectionUtils.SetDirectionTo(direction, GetTransformOrigin(defaultTransform), value);
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

    public static void SetDirectionTo(Direction dir, Transform from, Vector3 value)
    {
        switch (dir)
        {
            case Direction.Forward:
                from.forward = value;
                break;
            case Direction.Back:
                from.forward = -value;
                break;
            case Direction.Right:
                from.right = value;
                break;
            case Direction.Left:
                from.right = -value;
                break;
            case Direction.Up:
                from.up = value;
                break;
            case Direction.Down:
                from.up = -value;
                break;
            default:
                from.forward = value;
                break;
        }
    }
}
