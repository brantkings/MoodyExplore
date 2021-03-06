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


    public enum Mode
    {
        Movement,
        Command_Skill,
        Command_Focus,
        None
    }


    public delegate void DelPlayerEvent();

    public delegate void DelPlayerBoolEvent(bool set);
    public delegate void DelPlayerModeEvent(Mode newMode);

    public event DelPlayerModeEvent OnChangeCommandMode;
    public event DelPlayerEvent OnStartCommand;
    public event DelPlayerEvent OnStopCommand;

    [SerializeField]
    private MoodPawn _pawn;
    [SerializeField]
    private FocusController _focus;
    [SerializeField]
    private ThoughtSystemController _thought;
    public float maxVelocityPerBeat = 6f;
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
    private Vector3 _rotatingTarget;

    public float timeSlowOnThreat = 0.2f;
    public float timeSlowOnCommand = 0.02f;

    private bool _focusMode;

    private bool FocusMode
    {
        set
        {
            if(_focusMode != value)
            {
                _focusMode = value;
            }
        }
        get => _focusMode;
    }


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        _mainCamera = Camera.main;
    }

    void OnEnable()
    {
        _pawn.Threatenable.OnThreatAppear += ChangeThreat;
        _pawn.Threatenable.OnThreatRelief += ChangeThreat;
        _pawn.OnInterruptSkill += OnInterruptSkill;
        OnStartCommand += SolveThreatSlowDown;
        OnStopCommand += SolveThreatSlowDown;
    }


    void OnDisable()
    {
        _pawn.Threatenable.OnThreatAppear -= ChangeThreat;
        _pawn.Threatenable.OnThreatRelief -= ChangeThreat;
        _pawn.OnInterruptSkill -= OnInterruptSkill;
        OnStartCommand += SolveThreatSlowDown;
        OnStopCommand += SolveThreatSlowDown;
    }

    public MoodPawn Pawn => _pawn;

    public enum Join
    {
        And,
        Or
    }


    public struct KeyButtonState
    {

        private bool pressing;
        private bool justUp;
        private bool justDown;

        public bool Changed => JustUp || JustDown;

        public bool Pressing => pressing;

        public bool JustUp => justUp;

        public bool JustDown => justDown;

        public KeyButtonState(KeyCode code)
        {
            bool isPressed = IsKeyState(code);
            pressing = isPressed;
            justDown = isPressed && Input.GetKeyDown(code);
            justUp = !isPressed && Input.GetKeyUp(code);
        }

        public KeyButtonState(int mouseButton)
        {
            bool isPressed = Input.GetMouseButton(mouseButton);
            pressing = isPressed;
            justDown = isPressed && Input.GetMouseButtonDown(mouseButton);
            justUp = !isPressed && Input.GetMouseButtonUp(mouseButton);
        }


        public KeyButtonState(Join how, params KeyCode[] codes)
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

        public int GetValue()
        {
            return Pressing ? 1 : 0;
        }

        public static implicit operator bool(KeyButtonState e)
        {
            return e.pressing;
        }

        public static implicit operator int(KeyButtonState e)
        {
            return e.GetValue();
        }
    }

    public class BoolButtonState
    {
        public delegate bool DGetState();

        private Join _howToJoin;
        private DGetState[] _states;
        private bool _lastCheck;
        private bool _beforeCheck;

        public bool Changed => JustUp || JustDown;

        public void Recheck()
        {
            _beforeCheck = _lastCheck;

            if (_states == null || _states.Length <= 0) return;

            _lastCheck = _states[0]();
            bool press = _lastCheck;

            for (int i = 1; i < _states.Length; i++)
            {
                press = _states[i]();
                switch (_howToJoin)
                {
                    case Join.And:
                        _lastCheck &= press;
                        break;
                    default:
                        _lastCheck |= press;
                        break;

                }
            }
        }

        public bool Pressing => _lastCheck;

        public bool JustUp => _beforeCheck && !_lastCheck;

        public bool JustDown => !_beforeCheck && _lastCheck;

        public BoolButtonState(Join how, params DGetState[] codes)
        {
            _howToJoin = how;
            _states = codes;
            _lastCheck = false;

            Recheck();
        }

        public int GetValue()
        {
            return Pressing ? 1 : 0;
        }

        public static implicit operator bool(BoolButtonState e)
        {
            return e.Pressing;
        }

        public static implicit operator int(BoolButtonState e)
        {
            return e.GetValue();
        }
    }

    private struct AxisState
    {
        public KeyButtonState positive;
        public KeyButtonState negative;

        public AxisState(KeyButtonState negative, KeyButtonState positive)
        {
            this.negative = negative;
            this.positive = positive;
        }

        public int GetValue()
        {
            return positive - negative;
        }

        public int GetValueChanged()
        {
            if (Changed) return positive - negative;
            else return 0;
        }

        public bool Changed => positive.Changed || negative.Changed;

        public static implicit operator int(AxisState e)
        {
            return e.GetValue();
        }
    }

    private struct DirectionalState
    {
        public AxisState horizontal;
        public AxisState vertical;

        public KeyButtonState Up => vertical.positive;
        public KeyButtonState Down => vertical.negative;
        public KeyButtonState Right => horizontal.positive;
        public KeyButtonState Left => horizontal.negative;

        public Vector2 Get2DMoveAxis()
        {
            return vertical.GetValue() * Vector3.up + horizontal.GetValue() * Vector3.right;
        }

        public Vector3 GetMoveAxis()
        {
            Vector3 moveAxis = Vector3.zero;
            moveAxis += vertical.GetValue() * Vector3.forward;
            moveAxis += horizontal.GetValue() * Vector3.right;
            return moveAxis.normalized;
        }


        public int GetXAxis()
        {
            return horizontal.GetValue();
        }

        public int GetYAxis()
        {
            return vertical.GetValue();
        }

        public int GetXAxisChanged()
        {
            if (horizontal.Changed) return horizontal.GetValue();
            else return 0;
        }

        public int GetYAxisChanged()
        {
            if (vertical.Changed) return vertical.GetValue();
            else return 0;
        }

    }

    private class RecheckableAxis
    {
        public BoolButtonState positive;
        public BoolButtonState negative;

        public RecheckableAxis(BoolButtonState negative, BoolButtonState positive)
        {
            this.positive = positive;
            this.negative = negative;
        }

        public void Recheck()
        {
            positive.Recheck();
            negative.Recheck();
        }

        public int GetValue()
        {
            return positive - negative;
        }

        public int GetValueChanged()
        {
            if (positive.Changed || negative.Changed)
                return positive - negative;
            else return 0;
        }
    }


    private Vector3 _oldMousePos;

    void GetInputUpdate(out DirectionalState move, out Vector3 mouseMove, ref RecheckableAxis selectorMoveHelper, out AxisState focusAddAxis, out bool readyingWeapon, out KeyButtonState showCommand, out KeyButtonState executeAction, out KeyButtonState secondaryExecute, out KeyButtonState changeMode)
    {
        move = new DirectionalState();
        move.vertical = new AxisState(new KeyButtonState(Join.Or, KeyCode.DownArrow, KeyCode.S), new KeyButtonState(Join.Or, KeyCode.UpArrow, KeyCode.W));
        move.horizontal = new AxisState(new KeyButtonState(Join.Or, KeyCode.LeftArrow, KeyCode.A), new KeyButtonState(Join.Or, KeyCode.RightArrow, KeyCode.D));

        mouseMove = Input.mousePosition - _oldMousePos;
        //mouseMove = UnityEngine.m
        _oldMousePos = Input.mousePosition;

        focusAddAxis = new AxisState(new KeyButtonState(Join.Or, KeyCode.Q), new KeyButtonState(Join.Or, KeyCode.E));

        if (selectorMoveHelper == null)
        {
            selectorMoveHelper = new RecheckableAxis(
                new BoolButtonState(Join.Or, () => Input.mouseScrollDelta.y < 0f),
                new BoolButtonState(Join.Or, () => Input.mouseScrollDelta.y > 0f)
                );
        }
        else
        {
            selectorMoveHelper.Recheck();
        }

        readyingWeapon = false;
        showCommand = new KeyButtonState(KeyCode.Space);
        executeAction = new KeyButtonState(0);
        secondaryExecute = new KeyButtonState(1);
        changeMode = new KeyButtonState(KeyCode.Tab);
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
        return _pawn.Position;
    }

    private void StartSkillRoutine(MoodSkill skill, MoodItem item = null, Vector3 direction = default)
    {
        if (item != null)
        {
            Pawn.UseItem(item, skill);
        }
        if (skill != null)
        {
            StartCoroutine(ExecuteSkill(skill, direction));
        }
    }

    private IEnumerator ExecuteSkill(MoodSkill skill, Vector3 direction)
    {
        Debug.LogFormat("Starting command {0} {1}", skill.name, Time.time);
        OnStartCommand?.Invoke();
        yield return _pawn.ExecuteSkill(skill, direction);
        OnStopCommand?.Invoke();
        Debug.LogFormat("Ending command {0} {1}", skill.name, Time.time);
    }


    private void OnInterruptSkill(MoodPawn pawn, MoodSkill skill)
    {
        Debug.LogFormat("Interrupting command {0} {1}", skill.name, Time.time);
        OnStopCommand?.Invoke();
    }

    private bool IsExecutingCommand()
    {
        return _pawn.IsExecutingSkill();
    }

    private bool IsSkillNeedingStrategicCamera()
    {
        return _pawn.IsExecutingSkill() && _pawn.GetCurrentSkill().NeedsCameraUpwards();
    }

    RecheckableAxis _selectorMoveUpDownHelper;

    private void Update()
    {
        GetInputUpdate(out DirectionalState moveAxis, out Vector3 mouseAxis, ref _selectorMoveUpDownHelper, out AxisState focusAddAxis, out bool readyingWeapon,
            out KeyButtonState showCommandAction, out KeyButtonState executeAction, out KeyButtonState secondaryAction, out KeyButtonState changeModeButton);
        GetMouseInputUpdate(_mainCamera, GetPlayerPlaneOrigin(), ref _mouseWorldPosition);

        bool isInCommandMode = IsInCommandMode(showCommandAction.Pressing);
        bool isCameraUpwards = command.IsActivated() || IsSkillNeedingStrategicCamera();
        Mode currentMode = CheckCurrentMode(showCommandAction.Pressing);

        SetCommandMode(currentMode == Mode.Command_Skill);
        SetCameraMode(isCameraUpwards);
        if (isInCommandMode) //The command is open (holding space)
        {

            Vector3 inputDirection = _mouseWorldPosition - GetPlayerPlaneOrigin();
            Vector3 currentDirection = inputDirection;
            //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + currentDirection, Color.black, 0.02f);

            if (changeModeButton.JustDown)
            {
                FocusMode = !FocusMode;
                if(FocusMode)
                {
                    command.DeselectToNull(true, true);
                }
                else
                {
                    command.ShowCurrentSelected(true);
                }
                currentMode = CheckCurrentMode(showCommandAction.Pressing);
                return;
            }


            MoodSkill skill = command.GetCurrentSkill();
            MoodItem item = command.GetCurrentItem();
            MoodCommandOption option = command.GetCurrentCommandOption(); 
            skill?.SanitizeDirection(_pawn.Direction, ref currentDirection);

            if (currentMode == Mode.Command_Focus) //On Focus mode
            {
                //FocusCommand(moveAxis, moveAxis.vertical);
                command.UpdateCommandView(_pawn, skill, currentDirection, false);
                _thought.MovePointer(moveAxis.Get2DMoveAxis().normalized + (Vector2)mouseAxis, Time.unscaledDeltaTime);

                if(executeAction.JustDown)
                {
                    _thought.SelectWithPointer(Pawn);
                }
            }
            else if(currentMode == Mode.Command_Skill) //Not focus mode
            {
                //Make little orange arrow go to mouse
                inputDirectionFeedback.forward = inputDirection;
                
                command.UpdateCommandView(_pawn, skill, currentDirection, true);
                //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + currentDirection, Color.white, 0.02f);
                //Debug.DrawLine(GetPlayerPlaneOrigin(), GetPlayerPlaneOrigin() + pawn.Direction.normalized * currentDirection.magnitude, Color.red, 0.02f);
                

                int moveCommand = moveAxis.GetYAxisChanged() + _selectorMoveUpDownHelper.GetValue();
                int selectCommand = moveAxis.GetXAxisChanged();

                if (moveCommand > 0)
                {
                    command.MoveSelected(-1);
                }
                else if (moveCommand < 0)
                {
                    command.MoveSelected(1);
                }
                else if(selectCommand > 0)
                {
                    command.SelectCurrentOption();
                }
                else if(selectCommand < 0)
                {
                    if (IsBufferingSkill()) EndBufferingSkill();
                    command.Deselect();
                }
                else if (executeAction.JustDown)
                {
                    command.SelectCurrentOption();
                    if (skill != null)
                    {
                        if (skill.CanExecute(_pawn, currentDirection))
                        {
                            option.FeedbackExecute();
                            StartSkillRoutine(skill, item, currentDirection);
                        }
                        else if (skill.IsBufferable(Pawn))
                        {
                            StartBufferingSkill(option, skill, item, currentDirection);
                        }
                    }
                }

                if (secondaryAction.Pressing)
                {
                    _rotatingTarget = inputDirection.normalized;
                }
                else
                {
                    _rotatingTarget = Vector3.zero;
                }

                _pawn.SetVelocity(Vector3.zero);
                _pawn.RotateTowards(_rotatingTarget);

                if (!IsExecutingCommand())
                {
                    _pawn.SetLookAt(currentDirection);
                    _lookAtVector = currentDirection;
                    //pawn.SetHorizontalDirection(currentDirection);
                }
            }

            
        }
        else //The command is not open
        {
            FocusMode = false;

            if(!IsExecutingCommand())
            {
                //Check command shortcuts
                _rotatingTarget = Vector3.zero;
                Vector3 shortcutDirection = _pawn.Direction;
                foreach (var tuple in command.GetAllMoodSkills())
                {
                    if (Input.GetKeyDown(tuple.Item1.GetShortCut()) && tuple.Item1.CanExecute(_pawn, shortcutDirection))
                    {
                        StartSkillRoutine(tuple.Item1, tuple.Item2, shortcutDirection);
                    }
                }

                if (executeAction.JustDown)
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

                _pawn.SetLookAt(GetLookAtVector(Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up)));
                _pawn.SetVelocity(Vector3.ProjectOnPlane(ToWorldPosition(moveAxis.GetMoveAxis() * GetMaxVelocity()), Vector3.up));
                _pawn.RotateTowards(_rotatingTarget);
                //pawn.SetVelocity(moveAxis.GetMoveAxis() * 5f);

            }

        }


        //SolveCommandSlowDown(executeAction.Pressing, true);
        SolveCommandSlowDown(false, true);
    }

    private bool CanExecute(MoodSkill skill, MoodItem item, Vector3 skillDirection)
    {
        bool canSkill = skill != null && skill.CanExecute(Pawn, skillDirection);
        bool canItem = item == null || item.CanUse(Pawn, Pawn.Inventory);
        return canSkill;
    }

    private float GetMaxVelocity()
    {
        return maxVelocityPerBeat / TimeBeatManager.GetBeatLength();
    }

    private void FocusCommand(DirectionalState direction, AxisState addCommand)
    {
        if(_focus != null)
        {
            _focus.SelectNextFocusable(direction.GetXAxisChanged());
            _focus.ChangeLevelCurrentFocusable(addCommand.GetValueChanged());
        }
    }

    public bool HasAvailableSkills()
    {
        foreach (var skill in command.GetAllMoodSkills())
        {
            if (skill.Item1.CanExecute(_pawn, Vector3.zero))
            {
                return true;
            }
        }
        return false;
    }

    public Mode GetCurrentMode()
    {
        return _oldMode;
    }

    public Mode GetCurrentMode(bool pressingCommandButton)
    {
        if (IsInCommandMode(pressingCommandButton))
        {
            if (FocusMode) return Mode.Command_Focus;
            else return Mode.Command_Skill;
        }
        else return Mode.Movement;
    }

    Mode _oldMode;
    private Mode CheckCurrentMode(bool pressingCommandButton)
    {
        Mode currentMode = GetCurrentMode(pressingCommandButton);
        if(currentMode != _oldMode)
        {
            ChangedMode(currentMode);
            OnChangeCommandMode?.Invoke(currentMode);
            _oldMode = currentMode;
        }
        return currentMode;
    }

    private void ChangedMode(Mode newCurrentMode)
    {
        switch (newCurrentMode)
        {
            case Mode.Movement:
                command.SetActive(false);
                _thought.SetActivated(false);
                break;
            case Mode.Command_Skill:
                command.SetActive(true);
                _thought.SetActivated(false);
                break;
            case Mode.Command_Focus:
                command.SetActive(false);
                _thought.SetActivated(true);
                break;
            case Mode.None:
                break;
            default:
                break;
        }
        SetMouseMode(newCurrentMode);
    }


    public bool IsCommandOpen()
    {
        return command.IsActivated();
    }

    public bool IsInCommandMode(bool pressingButton)
    {
        return pressingButton;// || IsExecutingCommand();
    }

    public bool IsStunned()
    {
        return _pawn.IsStunned(MoodPawn.StunType.Action);
    }

    public bool IsManuallyRotating()
    {
        return _rotatingTarget != Vector3.zero && Vector3.Angle(_rotatingTarget, _pawn.Direction) > 1f;
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
    private bool CouldSlowdown()
    {
        return !IsManuallyRotating() && !IsBufferingSkill() && !IsStunned();
    }

    private bool ShouldThreatSlowdown()
    {
        return _pawn.Threatenable.IsThreatened() && !IsExecutingCommand() && CouldSlowdown();
    }
    private EventfulParameter<bool> _threatSlowedDown;
    private void SolveThreatSlowDown()
    {
        SolveSlowdown(timeSlowOnThreat, "PlayerThreat", ref _threatSlowedDown, ShouldThreatSlowdown());
    }

    private bool ShouldCommandSlowdown(bool pressingCommand, bool highlightingImpossibleCommand)
    {
        return IsCommandOpen() && HasAvailableSkills() && !(pressingCommand && highlightingImpossibleCommand) && CouldSlowdown();
    }

    private EventfulParameter<bool> _commandSlowedDown;
    private void SolveCommandSlowDown(bool pressingCommand, bool highlightingImpossibleCommand)
    {
        SolveSlowdown(timeSlowOnCommand, "PlayerCommand", ref _commandSlowedDown, ShouldCommandSlowdown(pressingCommand, highlightingImpossibleCommand));
    }


    private bool? _wasInCommand = null;
    private void SetCommandMode(bool set, bool feedback = true)
    {
        if(_wasInCommand != set)
        {
            if (!set)
            {
                EndBufferingSkill();
            }
            else
            {
            }
            _wasInCommand = set;

            OnChangeCommandMode?.Invoke(GetCurrentMode());
        }

    }

    private void SetMouseMode(Mode currentMode)
    {
        switch (currentMode)
        {
            case Mode.Movement:
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                break;
            case Mode.Command_Skill:
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                break;
            case Mode.Command_Focus:
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                break;
            case Mode.None:
                break;
            default:
                break;
        }
        //Debug.LogFormat("Setting mouse mode as visible? {0}", visible);
    }

    private void SetFocusMode(bool set)
    {
        FocusMode = set;
        _thought.SetActivated(set);
    }

    private void SetCommandMode(bool set)
    {
        command.SetActive(set);
    }

    #region Skill Buffer

    private Coroutine _skillBufferRoutine = null;
    private MoodSkill _bufferingSkill;

    private bool IsBufferingSkill()
    {
        return _skillBufferRoutine != null;
    }

    private void StartBufferingSkill(MoodCommandOption option, MoodSkill skill, MoodItem item, Vector3 direction)
    {
        if (IsBufferingSkill()) EndBufferingSkill();
        _bufferingSkill = skill;
        _skillBufferRoutine = StartCoroutine(SkillBuffer(option, skill, item, direction));
    }

    private void EndBufferingSkill()
    {
        _bufferingSkill = null;
    }

    private IEnumerator SkillBuffer(MoodCommandOption option, MoodSkill skill, MoodItem item, Vector3 direction)
    {
        option.FeedbackBuffer(true);
        while (skill == _bufferingSkill && skill != null && !skill.CanExecute(_pawn, direction))
        {
            yield return null;
            skill.SanitizeDirection(Pawn.Direction, ref direction);
        }
        Debug.LogFormat("Stopping buffer for skill {0}", skill);
        option.FeedbackBuffer(false);
        _skillBufferRoutine = null;
        if (_bufferingSkill == skill)
        {
            option.FeedbackExecute();
            StartSkillRoutine(skill, item, direction);
        }
    }
    #endregion


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
