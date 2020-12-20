using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Code.Animation.Humanoid;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using LHH.Utils;
using LHH.LeveledBehaviours.Sensors;
using LHH.Structures;
using System.Runtime.CompilerServices;
using System.Linq;

public interface IMoodPawnPeeker
{
    void SetTarget(MoodPawn pawn);
    void UnsetTarget(MoodPawn pawn);
}

public interface IMoodPawnBelonger
{
    MoodPawn GetMoodPawnOwner();
}

public interface IMoodPawnSetter : IMoodPawnBelonger
{
    void SetMoodPawnOwner(MoodPawn pawn);
}

public class MoodPawn : MonoBehaviour, IMoodPawnBelonger
{

    public delegate void DelMoodPawnEvent(MoodPawn pawn);
    public delegate void DelMoodPawnSkillEvent(MoodSkill skill, Vector3 direction);
    public delegate void DelMoodPawnSwingEvent(MoodSwing swing, Vector3 direction);
    public delegate void DelMoodPawnUndirectedSkillEvent(MoodSkill skill);

    public event DelMoodPawnEvent OnChangeStamina;

    public event DelMoodPawnUndirectedSkillEvent OnBeforeSkillUse;
    public event DelMoodPawnSwingEvent OnBeforeSwinging;
    public event DelMoodPawnSkillEvent OnUseSKill;
    public event DelMoodPawnUndirectedSkillEvent OnInterruptSkill;
    
    public KinematicPlatformer mover;
    public Animator animator;
    private LookAtIK _lookAtControl;
    [Space()]
    [SerializeField]
    private Health _health;
    [SerializeField]
    public MoodDamageModifier _undetectedDamageModifier;
    [SerializeField]
    private float knockBackMultiplier;
    [SerializeField]
    private MoodAttackFeedback _attackFeedback;
    [SerializeField]
    private SensorTarget _ownSensorTarget;
    [Space]
    [SerializeField]
    private GameObject toDestroyOnDeath;

    [SerializeField]
    private MoodReaction[] defaultReactions;


    [SerializeField]
    private MoodThreatenable _threatenable;
    public MoodThreatenable Threatenable
    {
        get
        {
            if(_threatenable == null)
            {
                _threatenable = gameObject.GetComponent<MoodThreatenable>();
                if(_threatenable == null)
                {
                    _threatenable = gameObject.AddComponent<MoodThreatenable>();
                }
            }
            return _threatenable;
        }
    }


    public Transform ObjectTransform
    {
        get
        {
            return mover.transform;
        }
    }

    [Space()]
    public float timeToMaxVelocity;
    public float timeToZeroVelocity = 0f;
    public float snapToTargetSpeedDelta = 0.1f;
    public float angleToBeAbleToAccelerate = 90f;
    public float turningTime = 0.1f;
    public float height = 2f;
    public float pawnRadius = 0.5f;

    public float extraRangeBase = 0f;
    public bool cantMoveWhileThreatened = true;
    
    [Space()]
    public float maxStamina;
    private float _stamina;
    public bool infiniteStamina;
    public float staminaRecoveryIdle;
    public float staminaRecoveryMoving;
    private Vector3 _damageAnimation;

    public Transform handPosition;

    [SerializeField]
    private DamageTeam damageTeam = DamageTeam.Neutral;

    public delegate void PawnEvent();

    private HashSet<MoodStance> _currentStances;


    public Vector3 Position => mover.Position;

    public Vector3 Up => mover.transform.up;

    public Vector3 Direction
    {
        get => mover.transform.forward;
        set
        {
            if(value != Vector3.zero)
                mover.transform.forward = value.normalized;
        }
        
    }


    public DamageTeam DamageTeam => damageTeam;

    private Vector3 _currentDirection;
    private Vector3 _directionVel;

    [SerializeField]
    private bool _debug;

    private bool Debugging
    {
        get
        {
#if UNITY_EDITOR
            return _debug;
#else
            return false;
#endif
        }
    }

    public bool Grounded
    {
        get
        {
            return mover.Grounded;
        }
    }

    public bool Walled
    {
        get
        {
            return mover.Walled;
        }
    }

    private void Awake() {
        _lookAtControl = animator.GetComponent<LookAtIK>();
    }

    private void OnEnable()
    {
        _movementLock.OnLock += OnLockMovement;
        _movementLock.OnUnlock += OnUnlockMovement;
        Threatenable.OnThreatAppear += OnThreatAppear;
        Threatenable.OnThreatRelief += OnThreatRelief;
        if (_health != null)
        {
            _health.OnBeforeDamage += OnBeforeDamage;
            _health.OnDamage += OnDamage;
            _health.OnDeath += OnDeath;
        }
    }
    

    private void OnDisable()
    {
        _movementLock.OnLock -= OnLockMovement;
        _movementLock.OnUnlock -= OnUnlockMovement;
        if(_health != null)
        {
            _health.OnBeforeDamage -= OnBeforeDamage;
            _health.OnDamage -= OnDamage;
            _health.OnDeath -= OnDeath;
        }
    }

    private void Start()
    {
        _stamina = maxStamina;
        _currentDirection = Direction;
        OnChangeStamina?.Invoke(this);
    }


    private void Update()
    {
        RecoverStamina(GetCurrentStaminaRecoverValue(), Time.deltaTime);

        //if (name.Contains("Player")) Debug.LogFormat("Before update {0} and {1} while Input is {2}", _currentSpeed, _currentDirection, _inputVelocity);
        UpdateMovement(_inputVelocity, _inputRotation, ref _currentSpeed, ref _currentDirection);
        //if (name.Contains("Player")) Debug.LogFormat("After update {0} and {1} while Input is {2}", _currentSpeed, _currentDirection, _inputVelocity);

        Direction = _currentDirection;
        /*Vector3 forward = mover.transform.forward;
        if (Vector3.Dot(forward, _currentDirection) < 0f) forward = Quaternion.Euler(0f,10f,0) * forward;
        Direction = Vector3.SmoothDamp(forward, _currentDirection, ref _directionVel, turningTime, float.MaxValue, Time.deltaTime);*/

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        mover.SetVelocity(_currentSpeed);

        ThreatFixedUpdate(_threatDirection);
    }

    public void PrepareForSwing(MoodSwing swing, Vector3 direction)
    {
        OnBeforeSwinging?.Invoke(swing, direction);
    }

    public void ShowSwing(MoodSwing swing, Vector3 direction)
    {
        if (_attackFeedback != null)
            _attackFeedback.DoFeedback(swing, direction);
        else Debug.LogErrorFormat("No attack feedback on {0}", this);
    }
    
    public void UsedSkill(MoodSkill skill, Vector3 direction)
    {
        OnUseSKill?.Invoke(skill, direction);
    }

    public void InterruptedSkill(MoodSkill skill)
    {
        OnInterruptSkill?.Invoke(skill);
    }

    public bool CanUseSkill(MoodSkill skill)
    {
        return !_stunLock.IsLocked() && skill.GetPluginPriority(this) > GetPlugoutPriority();
    }

    #region Health
    protected virtual void OnDamage(DamageInfo info, Health health)
    {
        if(info.amount != 0)
        {
            Stagger(info, health);
        }
    }

    public void Stagger(DamageInfo info, Health health)
    {
        AddStunLock("damage", info.stunTime);
        InterruptCurrentSkill();

        float animationDelay = 0.125f;
        float animationDuration = Mathf.Max(info.stunTime, info.durationKnockback, animationDelay);

        SetDamageAnimationTween(ObjectTransform.InverseTransformDirection(info.distanceKnockback.normalized) * 2f, animationDuration - animationDelay, animationDelay);
        Dash(info.distanceKnockback, info.durationKnockback, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        TweenMoverDirection(info.rotationKnockbackAngle, animationDuration).SetEase(Ease.OutQuad);
    }

    private void OnBeforeDamage(ref DamageInfo damage, Health damaged)
    {
        //if (_undetectedDamageModifier != null) Debug.LogFormat("Is {0} sensing origin {1}? {2}", this, damage.origin, IsSensing(damage.origin));
        if (_undetectedDamageModifier != null && !IsSensing(damage.origin))
        {
            _undetectedDamageModifier.ModifyDamage(ref damage);
        }
    }

    private void OnDeath(DamageInfo info, Health health)
    {
        Debug.LogFormat("{0} is dead.", this);
        if (toDestroyOnDeath != null) Destroy(toDestroyOnDeath);
    }
    #endregion

    #region Skills
    private MoodSkill _currentSkill;
    private Coroutine _currentSkillRoutine;
    private int _currentPlugoutPriority;


    public Coroutine ExecuteSkill(MoodSkill skill, Vector3 skillDirection)
    {
        _currentSkillRoutine = StartCoroutine(SkillRoutine(skill, skillDirection));
        return _currentSkillRoutine;
    }

    private IEnumerator SkillRoutine(MoodSkill skill, Vector3 skillDirection)
    {
        MarkUsingSkill(skill);
        yield return skill.ExecuteRoutine(this, skillDirection);
        UnmarkUsingSkill(skill);
    }

    public void InterruptCurrentSkill()
    {
        InterruptSkill(_currentSkill);
    }

    public void InterruptSkill(MoodSkill skill)
    {
        if(_currentSkill == skill && _currentSkill != null)
        {
            if(_currentSkillRoutine != null) StopCoroutine(_currentSkillRoutine);
            skill.Interrupt(this);
            _currentSkillRoutine = null;
            UnmarkUsingSkill(skill);
        }
    }

    public void MarkUsingSkill(MoodSkill skill)
    {
        _currentSkill = skill;
        OnBeforeSkillUse?.Invoke(skill);
    }

    public void UnmarkUsingSkill(MoodSkill skill)
    {
        if (_currentSkill == skill)
        {
            _currentSkill = null;
            SetPlugoutPriority(0);
        }
    }


    public void SetPlugoutPriority(int priority = 0)
    {
        _currentPlugoutPriority = priority;
    }

    public int GetPlugoutPriority()
    {
        if (_currentSkill == null) return -1;
        else return _currentPlugoutPriority;
    }

    public MoodSkill CurrentlyUsingSkill()
    {
        return _currentSkill;
    }
    #endregion

    #region Stances

    private HashSet<MoodStance> Stances
    {
        get
        {
            if(_currentStances == null) _currentStances = new HashSet<MoodStance>();
            return _currentStances;
        }
    }


    public IEnumerable<MoodReaction> GetActiveReactions()
    {
        foreach(MoodStance stance in Stances)
        {
            foreach(MoodReaction react in stance.GetReactions())
            {
                yield return react;
            }
        }
        if(defaultReactions != null)
        {
            foreach (MoodReaction react in defaultReactions) yield return react;
        }
    }
    
    public bool AddStance(MoodStance stance)
    {
        bool ok = Stances.Add(stance);
        if(ok) stance.ApplyStance(this, true);
        return ok;
    }

    
    public bool RemoveStance(MoodStance stance)
    {
        bool ok = Stances.Remove(stance);
        if(ok) stance.ApplyStance(this, false);
        return ok;
    }


    public bool ToggleStance(MoodStance stance)
    {
        if(Stances.Contains(stance)) return RemoveStance(stance);
        else return AddStance(stance);
    }



    public bool HasStance(MoodStance stance)
    {
        return Stances.Contains(stance);
    }

    public bool HasAnyStances(bool ifEmpty = true, params MoodStance[] stances)
    {
        if(stances == null || stances.Length == 0) return ifEmpty;

        foreach(var st in stances)
        {
            if(HasStance(st)) return true;
        }
        return false;
    }

    public bool HasAllStances(bool ifEmpty = false, params MoodStance[] stances)
    {
        if(stances == null || stances.Length == 0) return ifEmpty;

        foreach(var st in stances)
        {
            if(!HasStance(st)) return false;
        }
        return true;
    }
    #endregion

    #region Movement
    public event PawnEvent OnBeginMove;
    public event PawnEvent OnEndMove;

    private Vector3 _inputVelocity;
    private Vector3 _inputRotation;
    private Tween _movementTween;

    private Vector3 _currentSpeed;
    private Vector3 _movementDelta;

    private Tween _currentDash;

    private void UpdateMovement(Vector3 inputVelocity, Vector3 inputDirection, ref Vector3 speed, ref Vector3 direction)
    {
        if(inputVelocity.sqrMagnitude < 0.1f) //Wants to stop
        {
            UpdateMovementVector(ref speed, 0f, timeToZeroVelocity);

            //Maybe it is rotating while stopped
            if(inputDirection.sqrMagnitude >= 0.1f)
            {
                UpdateDirectionVector(ref direction, inputDirection, Time.deltaTime * 360f);
            }
        }
        else
        {

            Vector3 inputVelocityNormalized = inputVelocity.normalized;

            UpdateDirectionVector(ref direction, inputVelocityNormalized, Time.deltaTime * 360f);

            if (Vector3.Angle(inputVelocity, direction) < angleToBeAbleToAccelerate) //Already looking in the direction
            {
                UpdateMovementVector(ref speed, inputVelocity, timeToMaxVelocity);
            }
            else //Has to turn first
            {
                float maxVelocityTurning = 1f;
                float maxVelocityTurningSqrd = maxVelocityTurning * maxVelocityTurning;
                float smoothTimeTurning;
                if(speed.sqrMagnitude > maxVelocityTurningSqrd)
                {
                    smoothTimeTurning = timeToMaxVelocity;
                }
                else
                {
                    smoothTimeTurning = timeToZeroVelocity;
                }
                UpdateMovementVector(ref speed, inputVelocityNormalized * maxVelocityTurning, smoothTimeTurning);
            }
        }

    }

    private void UpdateDirectionVector(ref Vector3 direction, Vector3 targetDirection, float maxDelta)
    {
        float angleTurn = Vector3.SignedAngle(direction, targetDirection, Vector3.up);
        //Debug.LogFormat("Angle between [{3} <-> {4}] is {0} when total is {1} and max is {2}", Mathf.Sign(angleTurn) * Mathf.Max(Mathf.Abs(angleTurn), maxDelta), angleTurn, maxDelta, direction, targetDirection);
        angleTurn = Mathf.Sign(angleTurn) * Mathf.Min(Mathf.Abs(angleTurn), maxDelta);
        direction = Quaternion.Euler(0f, angleTurn, 0f) * direction;
    }

    private void UpdateMovementVector(ref Vector3 movement, float targetMagnitude, float smoothTime)
    {
        UpdateMovementVector(ref movement, movement.normalized * targetMagnitude, smoothTime);
    }

    private void UpdateMovementVector(ref Vector3 movement, Vector3 destination, float smoothTime)
    {
        movement = Vector3.SmoothDamp(movement, destination, ref _movementDelta, smoothTime);
        if ((movement - destination).sqrMagnitude < (snapToTargetSpeedDelta * snapToTargetSpeedDelta)) movement = destination;
    }

    private bool CanMove()
    {
        return !_movementLock.IsLocked() && !_stunLock.IsLocked();
    }

    private void SolveFinalVelocity(ref Vector3 finalVel)
    {
        if (_movementTween != null && _movementTween.IsActive() || !CanMove())
        {
            finalVel = Vector3.zero;
        }
        else
        {
            if (cantMoveWhileThreatened && Threatenable.IsThreatened())
            {
                finalVel = Vector3.zero;
            }
            /*else
            {
                if (_inputVelocity.sqrMagnitude > 0.001f)
                {
                    _directionTarget = Vector3.ProjectOnPlane(_inputVelocity, Vector3.up);
                }
            }*/
            
        }
    }

    private void StanceChangeVelocity(ref Vector3 velocity)
    {
        foreach(MoodStance stance in Stances)
        {
            stance.ModifyVelocity(ref velocity);
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        StanceChangeVelocity(ref velocity);
        _inputVelocity = velocity;
        SolveFinalVelocity(ref _inputVelocity);
    }

    public bool IsMoving()
    {
        return mover.Velocity.sqrMagnitude > 0.1f;
    }
    
    public Tween Dash(Vector3 direction, float duration, AnimationCurve curve)
    {
        if (_currentDash != null) _currentDash.KillIfActive();
        _currentDash = TweenMoverPosition(direction, duration).SetEase(curve);
        return _currentDash;
    }
    
    public Tween Dash(Vector3 direction, float duration, Ease ease)
    {
        if (_currentDash != null) _currentDash.KillIfActive();
        _currentDash = TweenMoverPosition(direction, duration).SetEase(ease);
        return _currentDash;
    }

    private Tween TweenMoverPosition(Vector3 movement, float duration)
    {
        CallBeginMove();
        _lerpPosition = mover.Position;
        return DOTween.To(GetPawnLerpPosition, SetPawnLerpPosition, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove);
    }

    private Tween TweenMoverDirection(Vector3 directionTo, float duration)
    {
        //CallBeginMove();
        return DOTween.To(GetPawnLerpDirection, SetPawnLerpDirection, directionTo, duration).SetId(this);//.OnKill(CallEndMove);
    }

    private Tween TweenMoverDirection(float angleAdd, float duration)
    {
        Vector3 directionAdd = Quaternion.Euler(0f, angleAdd, 0f) * Direction;
        return TweenMoverDirection(directionAdd, duration);
    }

    private void CallBeginMove()
    {
        OnBeginMove?.Invoke();
    }

    private void CallEndMove()
    {
        SolveFinalVelocity(ref _inputVelocity);
        OnEndMove?.Invoke();
    }

    private Vector3 _lerpPosition;

    private Vector3 GetPawnLerpPosition()
    {
        return _lerpPosition;
    }

    private void SetPawnLerpPosition(Vector3 set)
    {
        Vector3 diff = set - _lerpPosition;
        mover.AddExactNextFrameMove(set - _lerpPosition);
        _lerpPosition = set;
    }

    private Vector3 GetPawnLerpDirection()
    {
        return _currentDirection;
    }

    private void SetPawnLerpDirection(Vector3 set)
    {
        SetHorizontalDirection(set);
    }

    private void UpdateAnimation()
    {

    }

    public void StartSkillAnimation(MoodSkill skill)
    {
        animator.SetBool(UnityEngine.Random.Range(0f,1f) < 0.5? "Attack_Right" : "Attack_Left", true);
    }

    public void FinishSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack_Right", false);
        animator.SetBool("Attack_Left", false);
    }

    public void SetDamageAnimationTween(Vector3 direction, float duration, float delay)
    {
        _damageAnimation = Vector3.zero;
        SetDamageAnimation(direction);
        DOTween.To(GetDamageAnimationLerper, SetDamageAnimationLerper, direction, duration).From().SetEase(Ease.OutSine).SetDelay(delay);
    }

    private Vector3 GetDamageAnimationLerper()
    {
        return _damageAnimation;
    }

    private void SetDamageAnimationLerper(Vector3 v)
    {
        _damageAnimation = v;
        SetDamageAnimation(_damageAnimation);
    }

    public void SetDamageAnimation(Vector3 direction)
    {
        animator.SetFloat("DamageX", direction.x);
        animator.SetFloat("DamageZ", direction.z);
    }

    public void UnsetDamageAnimation()
    {
        SetDamageAnimation(Vector3.zero);
    }

    public void RotateTowards(Vector3 direction)
    {
        _inputRotation = direction;
    }

    public void StopRotating()
    {
        RotateTowards(Vector3.zero);
    }

    public void SetHorizontalDirection(Vector3 direction)
    {
        _currentDirection = direction;
        //Direction = Vector3.ProjectOnPlane(direction, Vector3.up);
    }

    public void SetLookAt(Vector3 direction)
    {
        _lookAtControl.LookAt(direction);
    }

    public Quaternion GetRotation()
    {
        return mover.transform.rotation;
    }


    #region Locking

    private Lock<string> _stunLock;
    private Lock<string> _movementLock;

    private void OnLockMovement()
    {
        mover.CancelVelocity();
    }
    private void OnUnlockMovement()
    {
        mover.CancelVelocity();
    }

    public void AddMovementLock(string str)
    {
        _movementLock.Add(str);
    }

    public void RemoveMovementLock(string str)
    {
        _movementLock.Remove(str);
    }

    private Coroutine _stunRoutine;

    public bool IsStunned()
    {
        return _stunLock.IsLocked();
    }

    private void EndStun()
    {
        if (_stunRoutine != null) StopCoroutine(_stunRoutine);
        _stunLock.RemoveAll();
    }

    public void AddStunLock(string str, float timeStunned)
    {
        _stunRoutine = StartCoroutine(StunLockRoutine(str, timeStunned));
    }

    private IEnumerator StunLockRoutine(string str, float timeStunned)
    {
        _stunLock.Add(str);
        yield return new WaitForSeconds(timeStunned);
        _stunLock.Remove(str);
        _stunRoutine = null;
    }
    #endregion

    #endregion

    #region Threat
    private Vector3 _threatDirection;
    private MoodSwing _swingThreat;

    public void StartThreatening(Vector3 direction, MoodSwing data)
    {
        _threatDirection = direction;
        _swingThreat = data;
    }

    public void StopThreatening()
    {
        _threatDirection = Vector3.zero;
        _swingThreat = null;
    }
    
    
    private void ThreatFixedUpdate(Vector3 threatDirection)
    {
        if (threatDirection == Vector3.zero)
        {
            ChangeThreatTarget(null);
            return;
        }
        if(_swingThreat)
        {
            ChangeThreatTargets(from result in _swingThreat.TryHitMerged(Position, GetRotation(), MoodGameManager.Instance.GetPawnBodyLayer()) select result.collider.GetComponentInParent<MoodThreatenable>());
        }
        else
        {
            ChangeThreatTarget(FindTarget(threatDirection, threatDirection.magnitude)?.GetComponentInParent<MoodThreatenable>());
        }
    }

    private HashSet<MoodThreatenable> _threatTarget = new HashSet<MoodThreatenable>();
    private void ChangeThreatTargets(IEnumerable<MoodThreatenable> targets)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (targets.Contains(threatened)) continue;
            threatened.RemoveThreat(gameObject);
        }
        _threatTarget.Clear();
        foreach(MoodThreatenable nextTarget in targets)
        {
            if(nextTarget != null && nextTarget != Threatenable)
            {
                if (_threatTarget.Add(nextTarget))
                {
                    nextTarget.AddThreat(gameObject);
                }
            }
        }
    }

    private void ChangeThreatTarget(MoodThreatenable nextTarget)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (threatened == nextTarget) continue;
            threatened.RemoveThreat(gameObject);
        }
        _threatTarget.Clear();
        if(nextTarget != null && nextTarget != Threatenable)
        {
            if(_threatTarget.Add(nextTarget))
            {
                nextTarget.AddThreat(gameObject);
            }
        }
    }


    private void OnThreatRelief(MoodThreatenable affected)
    {
        SolveFinalVelocity(ref _inputVelocity);
    }

    private void OnThreatAppear(MoodThreatenable affected)
    {
        SolveFinalVelocity(ref _inputVelocity);
    }


    #endregion

    #region Stamina

    private float GetCurrentStaminaRecoverValue()
    {
        bool isMoving = IsMoving();
        float value = isMoving ? staminaRecoveryMoving : staminaRecoveryIdle;
        foreach(MoodStance stance in Stances)
        {
            stance.ModifyStamina(ref value, isMoving);
        }
        return value;
    }

    private void RecoverStamina(float recovery, float timeDelta)
    {
        ChangeStamina(recovery * timeDelta);
    }
    
    public float GetStamina()
    {
        return infiniteStamina ? float.PositiveInfinity : _stamina;
    }

    public bool HasStamina(float cost)
    {    
        if (infiniteStamina) return true;
        else return _stamina >= cost;
    }

    public void DepleteStamina(float cost)
    {
        ChangeStamina(-cost);
    }

    private void ChangeStamina(float change)
    {
        float oldStamina = _stamina;
        _stamina = Mathf.Clamp(_stamina + change, 0f, maxStamina);
        if (oldStamina != _stamina)
        {
            OnChangeStamina?.Invoke(this);
        }
    }

    public float GetStaminaRatio()
    {
        if (infiniteStamina) return 1f;
        return GetStamina() / GetMaxStamina();
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }
    #endregion

    #region Places

    public Vector3 GetInstantiatePlace()
    {
        return handPosition != null ? handPosition.position : transform.position;
    }

    public Quaternion GetInstantiateRotation()
    {
        return GetRotation();
    }


    #endregion

    #region Sensors
    private bool _gotSensorTarget;
    public SensorTarget SensorTarget
    {
        get
        {
            if(_ownSensorTarget == null && !_gotSensorTarget)
            {
                _gotSensorTarget = true;
                _ownSensorTarget = GetComponentInChildren<SensorTarget>();
            }
            return _ownSensorTarget;
        }
    }


    public bool IsSensing(GameObject threat)
    {
        SensorTarget threatTarget = threat.GetComponentInParent<SensorTarget>();
        if (threatTarget != null) return IsSensing(threatTarget);
        MoodPawn pawn = threat.GetComponentInParent<IMoodPawnBelonger>()?.GetMoodPawnOwner();
        if(pawn != null)
        {
            return IsSensing(pawn);
        }
        else return false;
    }

    public bool IsSensing(MoodPawn pawn)
    {
        return IsSensing(pawn.SensorTarget);
    }

    public virtual bool IsSensing(SensorTarget target)
    {
        return false;
    }
    #endregion

    #region Targetting
    public Transform FindTarget(Vector3 direction, MoodSwing swing, LayerMask target)
    {
        return swing.TryHitGetBest(Position, GetRotation(), target, direction)?.collider.transform;
    }
    
    public Transform FindTarget(Vector3 direction, float range)
    {
        return Cast(direction, range, Position, transform.up, height);
    }

    private Transform Cast(Vector3 direction, float range,  Vector3 groundPosition, Vector3 capsuleDirection, float capsuleHeight)
    {
        Vector3 capsuleHeightVec = capsuleDirection * (capsuleHeight);
        Vector3 point1 = groundPosition;
        Vector3 point2 = groundPosition + capsuleHeightVec;
        
#if UNITY_EDITOR
        Vector3 distanceShoot = direction.normalized * range;
        Vector3 distPoint1 = point1 + distanceShoot;
        Vector3 distPoint2 = point2 + distanceShoot;
        Debug.DrawLine(distPoint1, distPoint2, Color.magenta, 0.02f);
        DebugUtils.DrawNormalStar(distPoint1, pawnRadius, Quaternion.identity, Color.magenta, 0.02f);
        DebugUtils.DrawNormalStar(distPoint2, pawnRadius, Quaternion.identity, Color.magenta, 0.02f);
#endif
        
        if (Physics.CapsuleCast(point1, point2, pawnRadius, direction.normalized, out RaycastHit hit, range,
            MoodGameManager.Instance.GetPawnBodyLayer()))
        {
            return hit.transform;
        }
        else
        {
            return null;
        }
    }

    public MoodPawn GetMoodPawnOwner()
    {
        return this;
    }
    #endregion


}
