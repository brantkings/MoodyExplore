using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UniformMovement : RigidbodyController
{
    public enum Direction
    {
        Forward,
        Up,
        Right
    }

    [SerializeField]
    private Direction _direction;
    [SerializeField]
    private float _velocityPerSecond = 1f;
    [SerializeField]
    private bool _inverted;
   
    private Transform DirectionGiver
    {
        get
        {
            return transform;
        }
    }

    private Vector3 GetDirection(Direction dir)
    {
        float invertMod = _inverted ? -1 : 1;
        switch (dir)
        {
            case Direction.Forward:
                return DirectionGiver.forward * invertMod;
            case Direction.Up:
                return DirectionGiver.up * invertMod;
            case Direction.Right:
                return DirectionGiver.right * invertMod;
            default:
                return DirectionGiver.forward * invertMod;
        }
    }

    public void SetDirection(Vector3 dir)
    {
        dir *= _inverted ? -1 : 1;
        switch (_direction)
        {
            case Direction.Forward:
                transform.forward = dir;
                break;
            case Direction.Up:
                transform.up = dir;
                break;
            case Direction.Right:
                transform.right = dir;
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        Body.MovePosition(Body.position + GetDirection(_direction) * _velocityPerSecond * Time.fixedDeltaTime);
    }

}
