using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Code.Animation.Humanoid;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using LHH.Utils;
using LHH.Sensors;
using LHH.Structures;
using System.Runtime.CompilerServices;
using System.Linq;

public interface IMoodPawnPeeker
{
    void SetTarget(MoodPawn pawn);
    void UnsetTarget(MoodPawn pawn);
}

public class MoodPawn : MonoBehaviour
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
    private float knockBackMultiplier;
    [SerializeField]
    private MoodAttackFeedback _attackFeedback;
    [Space]
    [SerializeField]
    private GameObject toDestroyOnDeath;


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

    [Space()]
    public FocusController focus;
    public SensorGroup sensorGroup;

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

    #region Health
    private void OnDamage(DamageInfo info, Health health)
    {
        Debug.LogFormat("{0} took damage! {1} -> dash is {2} distance with {3} duration.", this, info, info.distanceKnockback, info.durationKnockback);
        Dash(info.distanceKnockback, info.durationKnockback, AnimationCurve.Linear(0f, 0f, 1f, 1f));
    }

    private void OnDeath(DamageInfo info, Health health)
    {
        Debug.LogFormat("{0} is dead.", this);
        if (toDestroyOnDeath != null) Destroy(toDestroyOnDeath);
    }
    #endregion

    #region Skills
    private MoodSkill _currentSkill;
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
        }
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

    private void SolveFinalVelocity(ref Vector3 finalVel)
    {
        if (_movementTween != null && _movementTween.IsActive() || _movementLock.IsLocked())
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
        return TweenMoverPosition(direction, duration).SetEase(curve);
    }
    
    public Tween Dash(Vector3 direction, float duration, Ease ease)
    {
        return TweenMoverPosition(direction, duration).SetEase(ease);
    }

    private Tween TweenMoverPosition(Vector3 direction, float duration)
    {
        CallBeginMove();
        _lerpPosition = mover.Position;
        return DOTween.To(GetPawnLerpPosition, SetPawnLerpPosition, direction, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove);
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

    public void StartSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack", true);
    }

    public void FinishSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack", false);
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
    #endregion


}
