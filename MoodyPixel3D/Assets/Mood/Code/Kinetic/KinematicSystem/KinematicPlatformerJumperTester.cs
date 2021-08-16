using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicPlatformerJumperTester : AddonBehaviour<KinematicPlatformer>
{
    public ParabolaJumper Jumper;

    public float velocity = 10f;

    private enum KeyboardDirection
    {
        Up, Down, Left, Right
    }
    private int IsPressing(KeyboardDirection k)
    {
        switch (k)
        {
            case KeyboardDirection.Up:
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) return 1; else return 0;
            case KeyboardDirection.Down:
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return 1; else return 0;
            case KeyboardDirection.Left:
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return 1; else return 0;
            case KeyboardDirection.Right:
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return 1; else return 0;
            default:
                return 0;
        }
    }


    private void GetInput(out Vector2 moveAxis, out bool attacking, out bool jumping)
    {
        moveAxis = new Vector2(IsPressing(KeyboardDirection.Right) - IsPressing(KeyboardDirection.Left), IsPressing(KeyboardDirection.Up) - IsPressing(KeyboardDirection.Down)).normalized;
        attacking = Input.GetMouseButtonDown(0);
        jumping = Input.GetMouseButtonDown(1);
    }

    private void TransformToCamera(ref Vector2 vec)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraForwardProjected = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
        float angle = Vector3.SignedAngle(Vector3.forward, cameraForwardProjected, Vector3.up);

        vec = Quaternion.Euler(0f, 0f, -angle) * vec;
    }

    private void Start()
    {
        Application.targetFrameRate = 120;
    }

    private void Update()
    {
        bool attackPress, jumpPress;
        GetInput(out Vector2 directionAxis, out attackPress, out jumpPress);
        TransformToCamera(ref directionAxis);

        if(Jumper != null)
        {
            if (!Addon.Grounded) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jumper.Jump(new Vector3(directionAxis.x, 0f, directionAxis.y) * velocity);
                Move(Vector2.zero);
                return;
            }
        }
        Move(directionAxis);
    }


    private void Move(Vector2 axis)
    {
        axis *= velocity;
        Addon.SetVelocity(new Vector3(axis.x, 0f, axis.y));
    }
}
