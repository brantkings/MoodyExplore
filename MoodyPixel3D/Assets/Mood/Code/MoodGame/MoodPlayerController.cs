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
    public struct EventfulParameter<T>
    {
        private T _param;

        public delegate void DelOnChange(T changed);

        public void Update(T newValue, DelOnChange onChange)
        {
            if (_param.Equals(newValue)) return;
            _param = newValue;
            onChange(newValue);
        }
        
    }

    public delegate void DelPlayerEvent();

    public event DelPlayerEvent OnStartCommand;
    public event DelPlayerEvent OnStopCommand;
    
    [SerializeField]
    private MoodPawn pawn;
    public float maxVelocity = 5f;
    [SerializeField]
    private RangeSphere sphere;
    private Camera _mainCamera;



    public MoodCommandController command;
    public MoodInteractor interactor;
    public MoodCheckHUD checkHud;

    public Animator animatorCamera;
    public ScriptableEvent[] onCameraOut;
    public ScriptableEvent[] onCameraIn;
    public string animatorCameraCommandBoolean;
    public CinemachineBlendListCamera cameraBlendList;

    public Transform inputDirectionFeedback;

    private Vector3 _mouseWorldPosition;
    private MoodSkill _executingCommand;
    private Vector3 _rotatingTarget;

    public float timeSlowOnThreat = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        _mainCamera = Camera.main;
    }

    void OnEnable()
    {
        pawn.Threatenable.OnThreatAppear += ChangeThreat;
        pawn.Threatenable.OnThreatRelief += ChangeThreat;
        OnStartCommand += SolveThreatSlowDown;
        OnStopCommand += SolveThreatSlowDown;
    }
    
    void OnDisable()
    {
        pawn.Threatenable.OnThreatAppear -= ChangeThreat;
        pawn.Threatenable.OnThreatRelief -= ChangeThreat;
        OnStartCommand += SolveThreatSlowDown;
        OnStopCommand += SolveThreatSlowDown;
    }

    public MoodPawn Pawn => pawn;

    public struct ButtonState
    {
        public enum Join
        {
            And,
            Or
        }

        public bool pressing;
        public bool justUp;
        public bool justDown;

        public bool Changed => justUp || justDown;

        public ButtonState(KeyCode code)
        {
            bool isPressed = IsKeyState(code);
            pressing = isPressed;
            justDown = isPressed && Input.GetKeyDown(code);
            justUp = !isPressed && Input.GetKeyUp(code);
        }
        
        public ButtonState(int mouseButton)
        {
            bool isPressed = Input.GetMouseButton(mouseButton);
            pressing = isPressed;
            justDown = isPressed && Input.GetMouseButtonDown(mouseButton);
            justUp = !isPressed && Input.GetMouseButtonUp(mouseButton);
        }

        public ButtonState(Join how, params KeyCode[] codes)
        {
            bool press = IsKeyState(codes[0]);
            pressing = press;
            justDown = press && Input.GetKeyDown(codes[0]);
            justUp = !press && Input.GetKeyUp(codes[0]);

            for (int i = 1; i < codes.Length; i++)
            {
                KeyCode code = codes[i];
                press = IsKeyState(code);
                switch (how)
                {
                    case Join.And:
                        pressing &= press;
                        break;
                    default:
                        pressing |= press;
                        break;

                }
                justDown |= press && Input.GetKeyDown(code);
                justUp |= !press && Input.GetKeyUp(code);
            }

            if (pressing) justUp = false;
            else justDown = false;
        }

        private static bool IsKeyState(KeyCode code)
        {
            return Input.GetKey(code);
        }

        public static implicit operator bool(ButtonState e)
        {
            return e.pressing;
        }

        public static implicit operator int(ButtonState e)
        {
            return e.pressing ? 1 : 0;
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

    void GetInputUpdate(out DirectionalState move, out bool readyingWeapon, out ButtonState showCommand, out ButtonState executeAction, out ButtonState secondaryExecute)
    {
        move = new DirectionalState();
        readyingWeapon = false;
        showCommand = new ButtonState(KeyCode.Space);
        executeAction = new ButtonState(0);
        secondaryExecute = new ButtonState(1);

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

    private void StartSkillRoutine(MoodSkill skill, Vector3 direction)
    {
        StartCoroutine(ExecuteSkill(skill, direction));
    }

    private IEnumerator ExecuteSkill(MoodSkill skill, Vector3 direction)
    {
        Debug.LogFormat("Starting command {0}", Time.time);
        _executingCommand = skill;
        OnStartCommand?.Invoke();
        yield return pawn.ExecuteSkill(skill, direction);
        _executingCommand = null;
        OnStopCommand?.Invoke();
        Debug.LogFormat("Ending command {0}", Time.time);
    }

    private bool IsExecutingCommand()
    {
        return _executingCommand != null;
    }

    private bool IsSkillNeedingStrategicCamera()
    {
        return _executingCommand != null && _executingCommand.NeedsCameraUpwards();
    }


    private void Update()
    {
        GetInputUpdate(out DirectionalState moveAxis, out bool readyingWeapon, out ButtonState showCommandAction, out ButtonState executeAction, out ButtonState secondaryAction);
        GetMouseInputUpdate(_mainCamera, GetPlayerPlaneOrigin(), ref _mouseWorldPosition);


        command.SetActive(showCommandAction.pressing);

        bool isInCommandMode = IsInCommandMode();
        bool isCameraUpwards = command.IsActivated() || IsSkillNeedingStrategicCamera();


        SetCommandMode(isInCommandMode);
        SetCameraMode(isCameraUpwards);
        if (isInCommandMode) //The command is open
        {
            Vector3 inputDirection = _mouseWorldPosition - GetPlayerPlaneOrigin();
            Vector3 currentDirection = inputDirection;
            inputDirectionFeedback.forward = inputDirection;
            //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + currentDirection, Color.black, 0.02f);
            MoodSkill skill = command.GetCurrentSkill();

            //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + currentDirection, Color.white, 0.02f);
            //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + pawn.Direction.normalized * currentDirection.magnitude, Color.red, 0.02f);
            skill.SanitizeDirection(pawn.Direction, ref currentDirection);
            
            if (moveAxis.up.justDown)
            {
                command.MoveSelected(-1);
            }
            else if (moveAxis.down.justDown)
            {
                command.MoveSelected(1);
            }
            else if (executeAction.justDown)
            {
                Debug.LogFormat("Hey {0} ", skill.CanExecute(pawn, currentDirection));
                if (skill.CanExecute(pawn, currentDirection))
                {
                    StartSkillRoutine(skill, currentDirection);
                }
            }

            if(secondaryAction.pressing)
            {
                _rotatingTarget = inputDirection.normalized;
            }
            else
            {
                _rotatingTarget = Vector3.zero;
            }

            command.UpdateCommandView(pawn, currentDirection);
            pawn.SetVelocity(Vector3.zero);
            pawn.RotateTowards(_rotatingTarget);

            if (!IsExecutingCommand())
            {
                pawn.SetLookAt(currentDirection);
                _lookAtVector = currentDirection;
                //pawn.SetHorizontalDirection(currentDirection);
            }
        }
        else //The command is not open
        {
            //Check command shortcuts
            _rotatingTarget = Vector3.zero;
            Vector3 shortcutDirection = pawn.Direction;
            foreach(MoodSkill skill in command.GetMoodSkills())
            {
                if(Input.GetKeyDown(skill.GetShortCut()) && skill.CanExecute(pawn, shortcutDirection))
                {
                    StartSkillRoutine(skill, shortcutDirection);
                }
            }

            if (executeAction.justDown)
            {
                if (checkHud.IsShowing())
                {
                    checkHud.PressNext();
                }
                else if (interactor.HasInteractable())
                {
                    interactor.Interact();
                }
            }
            
            
            pawn.SetLookAt(GetLookAtVector(Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up)));
            pawn.SetVelocity(Vector3.ProjectOnPlane(ToWorldPosition(moveAxis.GetMoveAxis() * maxVelocity), Vector3.up));
            pawn.RotateTowards(_rotatingTarget);
            //pawn.SetVelocity(moveAxis.GetMoveAxis() * 5f);
        }

        SolveCommandSlowDown();
    }

    public bool IsCommandOpen()
    {
        return command.IsActivated();
    }

    public bool IsInCommandMode()
    {
        return IsCommandOpen() || IsExecutingCommand();
    }

    public bool IsStunned()
    {
        return pawn.IsStunned();
    }

    public bool IsManuallyRotating()
    {
        return _rotatingTarget != Vector3.zero && Vector3.Angle(_rotatingTarget, pawn.Direction) > 1f;
    }

    private Vector3 _lookAtVector;
    private Vector3 _lookAtVel;

    private Vector3 GetLookAtVector(Vector3 lookAtTarget)
    {
        _lookAtVector = Vector3.SmoothDamp(_lookAtVector, lookAtTarget, ref _lookAtVel, 0.1f, 5f, Time.deltaTime);
        return _lookAtVector;
    }


    private void ChangeThreat(MoodThreatenable moodPawn)
    {
        SolveThreatSlowDown();
    }

    private void SolveSlowdown(float timeSlow, string slowdownID, ref EventfulParameter<bool> boolean, bool updateValue)
    {
        boolean.Update(updateValue, (slowed) =>
        {
            Debug.LogFormat("{2} ({3}) slowdown {0} to {1}", timeSlow, slowdownID, slowed? "Adding" : "Removing", slowed);
            if (slowed) TimeManager.Instance.ChangeTimeDelta(timeSlow, slowdownID);
            else TimeManager.Instance.RemoveTimeDeltaChange(slowdownID);
        });
    }
    private bool ShouldThreatSlowdown()
    {
        return pawn.Threatenable.IsThreatened() && !IsExecutingCommand() && !IsManuallyRotating();
    }
    private EventfulParameter<bool> _threatSlowedDown;
    private void SolveThreatSlowDown()
    {
        SolveSlowdown(timeSlowOnThreat, "PlayerThreat", ref _threatSlowedDown, ShouldThreatSlowdown());
    }

    private bool ShouldCommandSlowdown()
    {
        return IsCommandOpen() && !IsExecutingCommand() && !IsManuallyRotating() && !IsStunned();
    }

    private EventfulParameter<bool> _commandSlowedDown;
    private void SolveCommandSlowDown()
    {
        SolveSlowdown(0.02f, "PlayerCommand", ref _commandSlowedDown, ShouldCommandSlowdown());
    }


    private bool? _wasInCommand = null;
    private void SetCommandMode(bool set, bool feedback = true)
    {
        if(_wasInCommand != set)
        {
            _wasInCommand = set;
            SetMouseMode(set);
        }

    }

    private void SetMouseMode(bool visible)
    {
        Debug.LogFormat("Setting mouse mode as visible? {0}", visible);
        UnityEngine.Cursor.visible = visible;
        UnityEngine.Cursor.lockState = visible? CursorLockMode.None : CursorLockMode.Locked;
    }


    private bool? _wasInCamera = null;

    private void SetCameraMode(bool upwards, bool feedback = true)
    {
        if(_wasInCamera != upwards)
        {
            animatorCamera.SetBool(animatorCameraCommandBoolean, upwards);
            if (feedback)
            {
                if (upwards) onCameraOut.Invoke(transform);
                else onCameraIn.Invoke(transform);
            }
            _wasInCamera = upwards;
            inputDirectionFeedback.gameObject.SetActive(upwards);
        }
    }
    
    private Vector3 ToWorldPosition(Vector3 vec)
    {
        return _mainCamera.transform.TransformDirection(vec);
    }
}
