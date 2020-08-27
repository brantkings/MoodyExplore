using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Cinemachine;
using Code.Animation.Humanoid;
using LHH.Utils;
using LHH.Utils.UnityUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MoodPlayerController : Singleton<MoodPlayerController>
{
    [SerializeField]
    private MoodPawn pawn;
    [SerializeField]
    private RangeSphere sphere;
    private Camera _mainCamera;


    public MoodCommandController command;

    public Enabler inCommand;
    public CinemachineBlendListCamera cameraBlendList;

    private Vector3 _mouseWorldPosition;
    private bool _executingCommand;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        _mainCamera = Camera.main;
    }

    public MoodPawn Pawn => pawn;

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
            bool isPressed = IsKeyState(code);
            pressed = isPressed;
            down = isPressed && Input.GetKeyDown(code);
            up = !isPressed && Input.GetKeyUp(code);
        }
        
        public ButtonState(int mouseButton)
        {
            bool isPressed = Input.GetMouseButton(mouseButton);
            pressed = isPressed;
            down = isPressed && Input.GetMouseButtonDown(mouseButton);
            up = !isPressed && Input.GetMouseButtonUp(mouseButton);
        }

        public ButtonState(Join how, params KeyCode[] codes)
        {
            bool press = IsKeyState(codes[0]);
            pressed = press;
            down = press && Input.GetKeyDown(codes[0]);
            up = !press && Input.GetKeyUp(codes[0]);

            for (int i = 1; i < codes.Length; i++)
            {
                KeyCode code = codes[i];
                press = IsKeyState(code);
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

        private static bool IsKeyState(KeyCode code)
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

    private struct DirectionalState
    {
        public ButtonState up;
        public ButtonState down;
        public ButtonState left;
        public ButtonState right;
        
        public Vector3 GetMoveAxis()
        {
            Vector3 moveAxis = Vector3.zero;
            moveAxis += up * Vector3.forward;
            moveAxis += left * Vector3.left;
            moveAxis += down * Vector3.back;
            moveAxis += right * Vector3.right;
            return moveAxis.normalized;
        }
    }

    void GetInputUpdate(out DirectionalState move, out bool readyingWeapon, out ButtonState showCommand, out ButtonState executeAction)
    {
        move = new DirectionalState();
        readyingWeapon = false;
        showCommand = new ButtonState(KeyCode.Space);
        executeAction = new ButtonState(0);


        move.up = new ButtonState(ButtonState.Join.Or, KeyCode.UpArrow, KeyCode.W);
        move.left = new ButtonState(ButtonState.Join.Or, KeyCode.LeftArrow, KeyCode.A);
        move.down = new ButtonState(ButtonState.Join.Or, KeyCode.DownArrow, KeyCode.S);
        move.right = new ButtonState(ButtonState.Join.Or, KeyCode.RightArrow, KeyCode.D);
        #if UNITY_EDITOR
        #endif
    }

    void GetMouseInputUpdate(Camera mainCamera, Vector3 playerPlanePosition, ref Vector3 position)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        float collisionLineFactor = Vector3.Dot(playerPlanePosition - ray.origin, Vector3.up) /
                                    Vector3.Dot(ray.direction, Vector3.up);
        position = ray.origin + ray.direction * collisionLineFactor;
        DebugUtils.DrawNormalStar(position, 0.5f, Quaternion.identity, Color.red, 0.02f);
        DebugUtils.DrawNormalStar(GetPlayerPlaneOrigin(), 0.5f, Quaternion.identity, Color.blue, 0.02f);
    }

    private Vector3 GetPlayerPlaneOrigin()
    {
        return pawn.Position;
    }

    private IEnumerator ExecuteCurrentCommand(Vector3 direction)
    {
        _executingCommand = true;
        Debug.LogFormat("Starting command {0}", Time.time);
        yield return command.ExecuteCurrent(pawn, direction);
        _executingCommand = false;
        Debug.LogFormat("Ending command {0}", Time.time);
    }

    private bool IsExecutingCommand()
    {
        return _executingCommand;
    }


    private void Update()
    {
        GetInputUpdate(out DirectionalState moveAxis, out bool readyingWeapon, out ButtonState showCommandAction, out ButtonState executeAction);
        GetMouseInputUpdate(_mainCamera, GetPlayerPlaneOrigin(), ref _mouseWorldPosition);
        Vector3 currentDirection = _mouseWorldPosition - GetPlayerPlaneOrigin();
        Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + currentDirection, Color.black, 0.02f);

        if (showCommandAction.down)
        {
            command.Activate(transform.position, 6f);
            _mouseWorldPosition = GetPlayerPlaneOrigin();
        }
        else if (showCommandAction.up)
        {
            command.Deactivate();
        }

        bool isInCommand = command.IsActivated() || IsExecutingCommand();

        SetCommandMode(isInCommand);
        if (isInCommand) //The command is open
        {
            if (moveAxis.up.down)
            {
                command.MoveSelected(-1);
            }
            else if (moveAxis.down.down)
            {
                command.MoveSelected(1);
            }
            else if (executeAction.down)
            {
                Debug.LogFormat("Hey {0} ", command.CanExecuteCurrent(pawn, currentDirection));
                if (command.CanExecuteCurrent(pawn, currentDirection))
                {
                    StartCoroutine(ExecuteCurrentCommand(currentDirection));
                    command.Deactivate();
                }
            }

            command.UpdateCommandView(pawn, currentDirection);
            pawn.SetVelocity(Vector3.zero);

            if (!IsExecutingCommand())
            {
                pawn.SetLookAt(currentDirection);
                pawn.SetDirection(currentDirection);
            }
        }
        else //The command is not open
        {
            pawn.SetLookAt(Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up));
            pawn.SetVelocity(ToWorldPosition((moveAxis.GetMoveAxis() * 5f)));
        }
        

    }

    private void SetCommandMode(bool set)
    {
        inCommand.SetActive(set);
        //_backToCameraControl.enabled = !set;
    }
    
    private Vector3 ToWorldPosition(Vector3 vec)
    {
        return _mainCamera.transform.TransformDirection(vec);
    }
}
