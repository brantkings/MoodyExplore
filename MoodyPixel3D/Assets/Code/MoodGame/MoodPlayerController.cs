using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MoodPlayerController : Singleton<MoodPlayerController>
{
    [SerializeField]
    private MoodPawn pawn;
    [SerializeField]
    private RangeSphere sphere;
    private Camera _mainCamera;

    public MoodCommandController command;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        _mainCamera = Camera.main;
    }

    public struct ButtonState
    {
        public enum Join
        {
            And,
            Or
        }

        public bool pressed;
        public bool up;
        public bool down;

        public bool Changed => up || down;

        public ButtonState(KeyCode code)
        {
            bool isPressed = IsState(code);
            pressed = isPressed;
            down = isPressed && Input.GetKeyDown(code);
            up = !isPressed && Input.GetKeyUp(code);
        }

        public ButtonState(Join how, params KeyCode[] codes)
        {
            bool press = IsState(codes[0]);
            pressed = press;
            down = press && Input.GetKeyDown(codes[0]);
            up = !press && Input.GetKeyUp(codes[0]);

            for (int i = 1; i < codes.Length; i++)
            {
                KeyCode code = codes[i];
                press = IsState(code);
                switch (how)
                {
                    case Join.And:
                        pressed &= press;
                        break;
                    default:
                        pressed |= press;
                        break;

                }
                down |= press && Input.GetKeyDown(code);
                up |= !press && Input.GetKeyUp(code);
            }

            if (pressed) up = false;
            else down = false;
        }

        private static bool IsState(KeyCode code)
        {
            return Input.GetKey(code);
        }

        public static implicit operator bool(ButtonState e)
        {
            return e.pressed;
        }

        public static implicit operator int(ButtonState e)
        {
            return e.pressed ? 1 : 0;
        }
    }

    void GetInputUpdate(out Vector3 moveAxis, out bool readyingWeapon, out ButtonState execute)
    {
        moveAxis = Vector3.zero;
        readyingWeapon = false;
        execute = new ButtonState(KeyCode.Space);

        moveAxis += new ButtonState(ButtonState.Join.Or, KeyCode.UpArrow, KeyCode.W) * Vector3.forward;
        moveAxis += new ButtonState(ButtonState.Join.Or, KeyCode.LeftArrow, KeyCode.A) * Vector3.left;
        moveAxis += new ButtonState(ButtonState.Join.Or, KeyCode.DownArrow, KeyCode.S) * Vector3.back;
        moveAxis += new ButtonState(ButtonState.Join.Or, KeyCode.RightArrow, KeyCode.D) * Vector3.right;
        moveAxis.Normalize();
        #if UNITY_EDITOR
        #endif
    }


    private bool showing;
    
    private void Update()
    {
        GetInputUpdate(out Vector3 moveAxis, out bool readyingWeapon, out ButtonState executeAction);
        pawn.mover.SetVelocity(ToWorldPosition((moveAxis * 5f)));

        if (executeAction.down)
        {
            command.Activate(transform.position, 6f);
        }
        else if (executeAction.up)
        {
            command.Deactivate();
        }

    }
    
    private Vector3 ToWorldPosition(Vector3 vec)
    {
        return _mainCamera.transform.TransformDirection(vec);
    }
}
