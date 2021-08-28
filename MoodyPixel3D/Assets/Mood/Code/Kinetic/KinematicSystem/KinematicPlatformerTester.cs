using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicPlatformerTester : AddonBehaviour<KinematicPlatformer>
{
    public float velocity = 10f;
    public float velocityAddStep = 2.5f;
    public float angleCameraChange = 10f;
    public Vector3 alignDirectionToPlaneOnly = Vector3.zero;
    public Camera _camera;
    private Vector3 _cameraDistance;
    private Vector2 _posMouseOld;

    [System.Serializable]
    private class SaveState
    {
        public Vector3 position;
        public Vector3 direction;
        public KeyCode key;

        internal void Save(Vector3 pos, Vector3 dir)
        {
            this.position = pos;
            this.direction = dir;
            Debug.LogFormat("Saving {0} and {1}", pos, dir);
        }
    }

    [SerializeField]
    private SaveState[] saveStates;
    public KeyCode saveKeyAdder;

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


    private void GetInput(out Vector2 moveAxis, out Vector2 mouseAxis, out bool firstButton, out bool secondButton)
    {
        moveAxis = new Vector2(IsPressing(KeyboardDirection.Right) - IsPressing(KeyboardDirection.Left), IsPressing(KeyboardDirection.Up) - IsPressing(KeyboardDirection.Down)).normalized;
        firstButton = Input.GetMouseButtonDown(0);
        secondButton = Input.GetMouseButtonDown(1);
        mouseAxis = (Vector2)Input.mousePosition - _posMouseOld;
        _posMouseOld = Input.mousePosition;
        
    }

    private Vector3 TransformToCamera(in Vector2 vec)
    {
        return Camera.main.transform.TransformDirection(new Vector3(vec.x, 0f, vec.y));
    }

    private void TransformToCameraParallelToGround(ref Vector2 vec)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraForwardProjected = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
        float angle = Vector3.SignedAngle(Vector3.forward, cameraForwardProjected, Vector3.up);

        vec = Quaternion.Euler(0f, 0f, -angle) * vec;
    }

    private void Start()
    {
        Application.targetFrameRate = 120;
        _posMouseOld = Input.mousePosition;
        if(_camera != null)
        {
            _cameraDistance = _camera.transform.position - Addon.Position;
        }
    }

    private void Update()
    {
        bool firstButton, secondButton;
        GetInput(out Vector2 directionAxis, out Vector2 mouseAxis, out firstButton, out secondButton);

        if (firstButton) velocity += velocityAddStep;
        else if (secondButton) velocity -= velocityAddStep;

        foreach(var state in saveStates)
        {
            if(Input.GetKeyDown(state.key))
            {
                if(Input.GetKey(saveKeyAdder))
                {
                    state.Save(Addon.Position, Addon.Direction);
                }
                else
                {
                    Addon.transform.position = state.position;
                    Addon.transform.forward = state.direction;
                    Addon.Direction = state.direction;
                }
            }
        }

        Vector3 directionCam = TransformToCamera(directionAxis);
        Vector3 mouseCam = TransformToCamera(mouseAxis);

        Vector3 direction = Addon.Direction;
        direction = Quaternion.Euler(new Vector3(0f, mouseAxis.x, 0f) * angleCameraChange) * direction;
        direction = Quaternion.AngleAxis(-mouseAxis.y * angleCameraChange, transform.right) * direction;
        direction = Vector3.ProjectOnPlane(direction, alignDirectionToPlaneOnly);
        Addon.Direction = direction;
        if(_camera != null)
        {
            _camera.transform.forward = Addon.Direction;
            _camera.transform.position = Addon.Position + Quaternion.LookRotation(Addon.Direction, Vector3.up) * _cameraDistance;
        }
        Addon.SetVelocity(directionCam.normalized * velocity);
    }


    private void MoveParallelToGround(Vector2 axis)
    {
        axis *= velocity;
        Addon.SetVelocity(new Vector3(axis.x, 0f, axis.y));
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.LogErrorFormat("Colliding {0} with {1} ({2})", this, collision.gameObject.name, collision.collider);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.LogErrorFormat("Colliding {0} with {1} ({2})", this, collision.gameObject.name, collision.collider);
    }
}
