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

public interface IBumpeable
{
    void OnBumped(GameObject origin, Vector3 force, Vector3 normal);
}

public class MoodPawn : MonoBehaviour, IMoodPawnBelonger, IBumpeable
{

    public delegate void DelMoodPawnEvent(MoodPawn pawn);
    public delegate void DelMoodPawnSkillEvent(MoodSkill skill, Vector3 direction);
    public delegate void DelMoodPawnSwingEvent(MoodSwing swing, Vector3 direction);
    public delegate void DelMoodPawnUndirectedSkillEvent(MoodSkill skill);

    public event DelMoodPawnEvent OnChangeStamina;

    public event DelMoodPawnUndirectedSkillEvent OnBeforeSkillUse;
    public event DelMoodPawnSwingEvent OnBeforeSwinging;
    public event DelMoodPawnSkillEvent OnUseSkill;
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
    private Transform _shakeFeedback;
    [SerializeField]
    private SensorTarget _ownSensorTarget;
    [Space]
    [SerializeField]
    private GameObject toDestroyOnDeath;

    [SerializeField]
    private MoodPawnStanceConfiguration pawnConfiguration;
    [SerializeField]
    private MoodReaction[] inherentReactions;
    [SerializeField]
    private ActivateableMoodStance[] flagInherentStances;


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
    public bool recoverStaminaWhileUsingSkill;
    public float staminaRecoveryIdle;
    public float staminaRecoveryMoving;
    private Vector3 _damageAnimation;

    public Transform handPosition;

    [SerializeField]
    private DamageTeam damageTeam = DamageTeam.Neutral;

    public delegate void PawnEvent();

    private HashSet<ActivateableMoodStance> _currentActivateableStances;


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

    public Vector3 Velocity
    {
        get
        {
            return mover.Velocity;
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

    public Health Health
    {
        get
        {
            return _health;
        }
    }

    private void Awake() {
        _lookAtControl = animator.GetComponent<LookAtIK>();
    }

    private void OnEnable()
    {
        _movementLock.OnLock += OnLockMovement;
        _movementLock.OnUnlock += OnUnlockMovement;
        mover.Walled.OnChanged += OnWalledChange;

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
        mover.Walled.OnChanged -= OnWalledChange;
        Threatenable.OnThreatAppear -= OnThreatAppear;
        Threatenable.OnThreatRelief -= OnThreatRelief;
        if (_health != null)
        {
            _health.OnBeforeDamage -= OnBeforeDamage;
            _health.OnDamage -= OnDamage;
            _health.OnDeath -= OnDeath;
        }

        //Remove current Threats
        ClearCurrentThreats();
    }

    private void Start()
    {
        _stamina = maxStamina;
        _currentDirection = Direction;
        OnChangeStamina?.Invoke(this);
    }


    private void Update()
    {
        if(ShouldRecoverStamina())
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

    #region Feedback

    public void PrepareForSwing(MoodSwing swing, Vector3 direction)
    {
        OnBeforeSwinging?.Invoke(swing, direction);
    }

    public void ShowSwing(MoodSwing swing, Vector3 direction)
    {
        if (_attackFeedback != null)
            _attackFeedback.DoFeedback(swing, direction);
    }

    public Transform GetShakeTransform()
    {
        return _shakeFeedback;
    }
    #endregion

    #region Health

    public Health.DamageResult Damage(DamageInfo dmg)
    {
        dmg.team = damageTeam;
        return _health.Damage(dmg);
    }

    protected virtual void OnDamage(DamageInfo info, Health health)
    {
        Debug.LogFormat("{0} takes damage with info {1}.", name , info);
        if (info.amount > 0 && pawnConfiguration != null) 
            AddFlag(pawnConfiguration.onDamage);
        Stagger(info, health);
    }

    private bool ShouldStaggerAnimation(DamageInfo info)
    {
        return info.shouldStaggerAnimation;
    }

    public void Stagger(DamageInfo info, Health health)
    {
        AddStunLockTimer(StunType.Action, "damage", info.stunTime);
        InterruptCurrentSkill();

        float animationDelay = 0.125f;
        float animationDuration = Mathf.Max(info.stunTime, info.durationKnockback, animationDelay);

        if(ShouldStaggerAnimation(info)) 
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


    public bool IsExecutingSkill()
    {
        return _currentSkill != null && _currentSkillRoutine != null;
    }

    public Coroutine ExecuteSkill(MoodSkill skill, Vector3 skillDirection)
    {
        if (IsExecutingSkill()) InterruptCurrentSkill();
        _currentSkillRoutine = StartCoroutine(SkillRoutine(skill, skillDirection));
        return _currentSkillRoutine;
    }

    private IEnumerator SkillRoutine(MoodSkill skill, Vector3 skillDirection)
    {
        MarkUsingSkill(skill);
        if(pawnConfiguration?.stanceOnSkill != null) AddStance(pawnConfiguration.stanceOnSkill);
        yield return skill.ExecuteRoutine(this, skillDirection);
        if (pawnConfiguration?.stanceOnSkill != null) RemoveStance(pawnConfiguration.stanceOnSkill);
        _currentSkillRoutine = null;
        UnmarkUsingSkill(skill);
    }

    public void InterruptCurrentSkill()
    {
        if(_currentSkill != null)
        {
            Debug.LogFormat("{0} gonna interrupt current skill {1}", this.name, _currentSkill?.name);
            InterruptSkill(_currentSkill);
        }
    }

    public void InterruptSkill(MoodSkill skill)
    {
        if(_currentSkill == skill && _currentSkill != null)
        {
            Debug.LogFormat("{0} gonna interrupt skill {1}", this.name, skill?.name);
            if (_currentSkillRoutine != null) StopCoroutine(_currentSkillRoutine);
            if (pawnConfiguration?.stanceOnSkill != null) RemoveStance(pawnConfiguration.stanceOnSkill);
            _currentSkillRoutine = null;
            skill.Interrupt(this);
            OnInterruptSkill?.Invoke(skill);
            UnmarkUsingSkill(skill);
        }
    }

    public void MarkUsingSkill(MoodSkill skill)
    {
        //Debug.LogFormat("{0} mark using skill {1}", this.name, skill?.name);
        _currentSkill = skill;
        OnBeforeSkillUse?.Invoke(skill);
    }

    public void UnmarkUsingSkill(MoodSkill skill)
    {
        if (_currentSkill == skill)
        {
            //Debug.LogFormat("{0} unmark using skill {1}", this.name, skill?.name);
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

    public MoodSkill GetCurrentSkill()
    {
        return _currentSkill;
    }
    public void UsedSkill(MoodSkill skill, Vector3 direction)
    {
        Debug.LogFormat("{0} executes {1} {2}!", name, skill.name, Time.time);
        OnUseSkill?.Invoke(skill, direction);
    }

    public bool CanUseSkill(MoodSkill skill)
    {
        return !IsStunned(StunType.Action) && skill.GetPluginPriority(this) > GetPlugoutPriority();
    }
    #endregion

    #region Stances


    private int _stancesSemaphore;
    private bool IsRunningThroughStances()
    {
        return _stancesSemaphore > 0;
    }

    private void AddFlagStancesSemaphore(int add)
    {
        _stancesSemaphore += add;
        if (_stancesSemaphore <= 0 && add < 0) DealWithTempFlags();
    }

    private HashSet<ActivateableMoodStance> AddedStances
    {
        get
        {
            if(_currentActivateableStances == null) _currentActivateableStances = new HashSet<ActivateableMoodStance>();
            return _currentActivateableStances;
        }
    }

    private IEnumerable<ConditionalMoodStance> ConditionalStances
    {
        get
        {
            if (pawnConfiguration != null)
                foreach (ConditionalMoodStance stance in pawnConfiguration.conditionalStances) yield return stance;
        }
    }

    private IEnumerable<ConditionalMoodStance> GetActiveConditionalMoodStances()
    {
        foreach (ConditionalMoodStance stance in ConditionalStances)
            if (stance.IsItOn(this)) yield return stance;
    }

    public IEnumerable<MoodStance> AllActiveStances
    {
        get
        {
            AddFlagStancesSemaphore(1);
            foreach (ActivateableMoodStance stance in AddedStances) yield return stance;
            AddFlagStancesSemaphore(-1);
            foreach (ConditionalMoodStance stance in GetActiveConditionalMoodStances()) yield return stance;
        }
    }



    public IEnumerable<MoodReaction> GetActiveReactions()
    {
        foreach(MoodStance stance in AllActiveStances)
        {
            foreach(MoodReaction react in stance.GetReactions())
            {
                yield return react;
            }
        }
        if (pawnConfiguration != null)
        {
            foreach (MoodReaction react in pawnConfiguration.reactions) yield return react;
        }
        if (inherentReactions != null)
        {
            foreach (MoodReaction react in inherentReactions) yield return react;
        }
    }
    
    public bool AddStance(ActivateableMoodStance stance)
    {
        bool ok = AddedStances.Add(stance);
        if (ok)
        {
            Debug.LogFormat("[STANCE] {0} added stance {1}", name, stance.name);
            stance.ApplyStance(this, true);
        }
        return ok;
    }

    
    public bool RemoveStance(ActivateableMoodStance stance)
    {
        bool ok = AddedStances.Remove(stance);
        if (ok)
        {
            Debug.LogFormat("[STANCE] {0} removed stance {1}", name, stance.name);
            stance.ApplyStance(this, false);
        }
        return ok;
    }

    private List<MoodEffectFlag> _tempFlags = new List<MoodEffectFlag>(8);

    private void DealWithTempFlags()
    {
        foreach (MoodEffectFlag flag in _tempFlags) AddFlag(flag);
        _tempFlags.Clear();
    }

    private HashSet<ActivateableMoodStance> _toRemoveFromAddedFlags = new HashSet<ActivateableMoodStance>();

    public bool AddFlag(MoodEffectFlag flag)
    {
        if (flag == null) 
            return false;
        if(IsRunningThroughStances())
        {
            _tempFlags.Add(flag);
            return false;
        }
        bool changedAnyFlag = false;
        foreach (var stance in AllFlaggeableStances().Where(x => x.HasFlagToActivate(flag)))
        {
            Debug.LogFormat("{0} added stance {1} due to flag {2}.", this.name, stance, flag.name);
            changedAnyFlag = AddStance(stance) || changedAnyFlag;
        }
        foreach (var stance in AddedStances.Where(x => x.HasFlagToDeactivate(flag)))
        {
            _toRemoveFromAddedFlags.Add(stance);
        }
        foreach(var stance in _toRemoveFromAddedFlags)
        {
            Debug.LogFormat("{0} removed stance {1} due to flag {2}.", this.name, stance, flag.name);
            changedAnyFlag = RemoveStance(stance) || changedAnyFlag;
        }
        _toRemoveFromAddedFlags.Clear();
        return changedAnyFlag;
    }

    public bool AddFlags(IEnumerable<MoodEffectFlag> flags)
    {
        if (flags != null)
        {
            bool didIt = false;
            foreach (var flag in flags)
            {
                didIt = didIt || AddFlag(flag);
            }
            return didIt;
        }
        else return false;
    }

    public bool HasFlag(MoodEffectFlag flag)
    {
        return AddedStances.Any(x => x.HasFlagToActivate(flag));
    }


    public IEnumerable<ActivateableMoodStance> AllFlaggeableStances()
    {
        foreach (ActivateableMoodStance stance in flagInherentStances) yield return stance;
        foreach (ActivateableMoodStance stance in pawnConfiguration?.flaggeableStances) yield return stance;
    }


    public bool ToggleStance(ActivateableMoodStance stance)
    {
        if(AddedStances.Contains(stance)) return RemoveStance(stance);
        else return AddStance(stance);
    }

    public bool HasStance(MoodStance stance)
    {
        return AddedStances.Contains(stance) || HasConditionalStance(stance as ConditionalMoodStance);
    }

    private bool HasConditionalStance(ConditionalMoodStance stance)
    {
        if (stance == null) return false;
        return stance.IsItOn(this);
    }

    public bool IsInNeutralStance()
    {
        foreach(MoodStance stance in AllActiveStances)
        {
            if (!stance.IsNeutralStance()) return false;
        }
        return true;
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
    public event PawnEvent OnNextBeginMove;
    public event PawnEvent OnEndMove;
    public event PawnEvent OnNextEndMove;
    public event PawnEvent OnCompleteMove;
    public event PawnEvent OnNextCompleteMove;

    private Vector3 _inputVelocity;
    private Vector3 _inputRotation;
    private Tween _movementTween;

    private Vector3 _currentSpeed;
    private Vector3 _movementDelta;

    private Tween _currentDash;
    private Tween _currentFakeHeightHop;

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
        return !IsStunned(StunType.Movement) && !IsStunned(StunType.Action);
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

    private void OnWalledChange(bool change)
    {
        if(change)
        {
            if(IsDashing())
            {
                Bump(mover.Velocity);
            }
        }
    }

    private Vector3 GetKnockbackForce(Vector3 bumpVelocity, bool isGuilty, out float duration)
    {
        duration = GetKnockbackDuration(bumpVelocity, isGuilty);
        Vector3 bumpForce = bumpVelocity.normalized * 1f;
        if (isGuilty) return -bumpForce;
        else return bumpForce;
    }

    private float GetKnockbackDuration(Vector3 bumpVelocity, bool isGuilty)
    {
        return isGuilty ? 0.5f : 0.125f;
    }

    private MoodReaction.ReactionInfo MakeReactionInfo(Vector3 velocity, GameObject origin, Vector3 normal, bool isGuilty)
    {
        MoodReaction.ReactionInfo info = new MoodReaction.ReactionInfo(origin, GetKnockbackForce(velocity, isGuilty, out float duration), duration).SetNormal(normal);
        return info;
    }

    private void HandleBumpInfo(MoodReaction.ReactionInfo bumpInfo)
    {
        foreach (MoodReaction react in GetActiveReactions())
        {
            if (react.CanReact(this, bumpInfo, MoodReaction.ActionType.Bumped))
            {
                react.ReactToBump(ref bumpInfo, this);
            }
        }
        AddStunLockTimer(StunType.Action, "bump", bumpInfo.knockbackDuration);
        SetDamageAnimationTween(bumpInfo.direction.normalized * 0.5f, bumpInfo.knockbackDuration, 0f);
        Dash(bumpInfo.direction, bumpInfo.knockbackDuration, Ease.OutSine);
    }

    private void Bump(Vector3 currentVelocity)
    {
        Collider col = mover.WhatIsInThere(currentVelocity.normalized * 0.25f, KinematicPlatformer.CasterClass.Side, out RaycastHit hit);
        if(col != null)
        {
            col.GetComponentInParent<IBumpeable>()?.OnBumped(this.gameObject, currentVelocity, hit.normal);
            HandleBumpInfo(MakeReactionInfo(currentVelocity, col.gameObject, hit.normal, true));
        }
    }

    public void OnBumped(GameObject origin, Vector3 velocity, Vector3 normal)
    {
        MoodReaction.ReactionInfo info = MakeReactionInfo(velocity, origin, normal, false);
        HandleBumpInfo(info);
    }

    private void StanceChangeVelocity(ref Vector3 velocity)
    {
        foreach(ActivateableMoodStance stance in AddedStances)
        {
            if(stance != null)
            {
                foreach (var mod in stance.GetAllModifiers<IMoodPawnModifierVelocity>()) mod.ModifyVelocity(ref velocity);
            }
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        StanceChangeVelocity(ref velocity);
        _inputVelocity = velocity;
        SolveFinalVelocity(ref _inputVelocity);
    }

    public float GetHeightFromGround()
    {
        return GetPawnFakeHeight(); //TODO yeah this assumes no verticality in the world
    }

    public bool IsMoving()
    {
        return mover.Velocity.sqrMagnitude > 0.1f;
    }

    public bool IsDashing()
    {
        return _currentDash.IsNotNullAndMoving();
    }

    public Tween FakeHop(float height, float durationIn, float durationOut)
    {
        Sequence seq = DOTween.Sequence();
        seq.Insert(0f, TweenFakeHeight(height, durationIn, Ease.OutCirc));
        seq.Insert(durationIn, TweenFakeHeight(0f, durationOut, Ease.InCirc));
        _currentFakeHeightHop = seq;
        return seq;
    }

    public float CurrentDashDuration()
    {
        if (_currentDash != null)
        {
            return _currentDash.Duration();
        }
        else return 0f;
    }
    
    public void Dash(Vector3 direction, float duration, AnimationCurve curve)
    {
        if (_currentDash != null) _currentDash.KillIfActive();
        _currentDash = TweenMoverPosition(direction, duration).SetEase(curve);
    }
    
    public void Dash(Vector3 direction, float duration, Ease ease)
    {
        if (_currentDash != null) _currentDash.KillIfActive();
        _currentDash = TweenMoverPosition(direction, duration).SetEase(ease);
    }

    private Tween TweenFakeHeight(float height, float duration, Ease ease)
    {
        _currentFakeHeightHop.CompleteIfActive();
        _currentFakeHeightHop = null;

        _currentFakeHeightHop = DOTween.To(GetPawnFakeHeight, SetPawnFakeHeight, height, duration).SetEase(ease);
        return _currentFakeHeightHop;
    }

    private Tween TweenMoverPositionIgnoreGravity(Vector3 movement, float duration, bool callBeginMove = true)
    {
        _lerpPosition = mover.Position;
        Tween t = DOTween.To(GetPawnLerpPosition, SetPawnLerpPositionIgnoreGravity, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove).OnComplete(CallCompleteMove);
        if (callBeginMove) t.OnStart(CallBeginMove);
        return t;
    }

    private Tween TweenMoverPosition(Vector3 movement, float duration)
    {
        _lerpPosition = mover.Position;
        return DOTween.To(GetPawnLerpPosition, SetPawnLerpPosition, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove).OnStart(CallBeginMove).OnComplete(CallCompleteMove);
    }

    private Tween TweenMoverDirection(float angleAdd, float duration)
    {
        Vector3 directionAdd = Quaternion.Euler(0f, angleAdd, 0f) * Direction;
        return TweenMoverDirection(directionAdd, duration);
    }

    private Tween TweenMoverDirection(Vector3 directionTo, float duration)
    {
        return DOTween.To(GetPawnLerpDirection, SetPawnLerpDirection, directionTo, duration).SetId(this);//.OnKill(CallEndMove).OnStart(CallBeginMove);
    }


    private void CallBeginMove()
    {
        //Debug.LogWarningFormat("Start move {0}, {1}", this, Time.time);
        OnBeginMove?.Invoke();
        OnNextBeginMove?.Invoke();
        OnNextBeginMove = null;
    }

    private void CallEndMove()
    {
        //Debug.LogWarningFormat("End move {0}, {1}", this, Time.time);
        SolveFinalVelocity(ref _inputVelocity);
        OnEndMove?.Invoke();
        //Debug.LogFormat("Gonna do next end move on {0} which is {1}", this, OnNextEndMove?.GetInvocationList().Count());
        OnNextEndMove?.Invoke();
        OnNextEndMove = null;
    }


    private void CallCompleteMove()
    {
        OnCompleteMove?.Invoke();
        OnNextCompleteMove?.Invoke();
        OnNextCompleteMove = null;
    }

    private Vector3 _lerpPosition;

    private Vector3 GetPawnLerpPosition()
    {
        return _lerpPosition;
    }

    private void SetPawnLerpPositionIgnoreGravity(Vector3 set)
    {
        Vector3 diff = set - _lerpPosition;
        mover.AddExactNextFrameMove(set - _lerpPosition - KinematicPlatformer.GetGravityForce() * Time.fixedDeltaTime);
        _lerpPosition = set;
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

    private float GetPawnFakeHeight()
    {
        return animator.transform.position.y;
    }

    private void SetPawnFakeHeight(float h)
    {
        Vector3 animPos = animator.transform.position;
        animPos.y = h;
        animator.transform.position = animPos;
    }

    private void SetPawnLerpDirection(Vector3 set)
    {
        SetHorizontalDirection(set);
    }

    private void UpdateAnimation()
    {

    }

    private float timeStampAttack;
    private string lastAttack;

    public void StartSkillAnimation(MoodSkill skill)
    {
        string attackStr;
        if(Time.time - timeStampAttack < 1f)
        {
            attackStr = lastAttack == "Attack_Right" ? "Attack_Left" : "Attack_Right";
        }
        else
        {
            attackStr = UnityEngine.Random.Range(0f, 1f) < 0.5 ? "Attack_Right" : "Attack_Left";
        }

        lastAttack = attackStr;
        timeStampAttack = Time.time;

        animator.SetBool(attackStr, true);
        //Debug.LogErrorFormat("Start attack animation by {0} {1}", skill.name, Time.frameCount);
    }

    public void FinishSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack_Right", false);
        animator.SetBool("Attack_Left", false);
        //Debug.LogErrorFormat("Finish attack animation by {0} {1}", skill.name, Time.frameCount);
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

    private Lock<string> _actionStunLock = new Lock<string>();
    private Lock<string> _reactionStunLock = new Lock<string>();
    private Lock<string> _movementLock = new Lock<string>();

    public enum StunType
    {
        Action,
        Reaction,
        Movement,
        None
    }

    private void OnLockMovement()
    {
        mover.CancelVelocity();
    }
    private void OnUnlockMovement()
    {
        mover.CancelVelocity();
    }


    private Dictionary<StunType, Dictionary<string,Coroutine>> _stunRoutines = new Dictionary<StunType, Dictionary<string, Coroutine>>(4);

    private Lock<string> GetLock(StunType type)
    {
        switch (type)
        {
            case StunType.Action:
                return _actionStunLock;
            case StunType.Reaction:
                return _reactionStunLock;
            case StunType.Movement:
                return _movementLock;
            default:
                return null;
        }
    }

    public bool IsStunned(StunType type)
    {
        return GetLock(type).IsLocked();
    }

    private void EndStun(StunType type)
    {
        RemoveStunLockTimer(type);
        GetLock(type).RemoveAll();
    }

    public void AddStunLock(StunType type, string str)
    {
        GetLock(type).Add(str);
    }

    public void RemoveStunLock(StunType type, string str)
    {
        GetLock(type).Remove(str);
    }

    private void RemoveStunLockTimer(StunType type)
    {
        if (_stunRoutines.ContainsKey(type))
        {
            foreach(var routine in _stunRoutines[type].Values)
                StopCoroutine(routine);
            _stunRoutines.Remove(type);
        }
    }

    public void AddStunLockTimer(StunType type, string str, float timeStunned)
    {
        if(!_stunRoutines.ContainsKey(type))
        {
            _stunRoutines.Add(type, new Dictionary<string, Coroutine>(4));
        }
        Dictionary<string, Coroutine> dict = _stunRoutines[type];
        if(dict.ContainsKey(str))
        {
            StopCoroutine(dict[str]);
            dict.Remove(str);
        }
        dict.Add(str, StartCoroutine(StunLockRoutine(type, str, timeStunned)));
    }

    private IEnumerator StunLockRoutine(StunType type, string str, float timeStunned)
    {
        GetLock(type).Add(str);
        if(timeStunned>0) yield return new WaitForSeconds(timeStunned);
        GetLock(type).Remove(str);
        _stunRoutines.Remove(type);
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
        ClearCurrentThreats(targets);
        foreach(MoodThreatenable nextTarget in targets)
        {
            if(nextTarget != null && nextTarget != Threatenable)
            {
                if (_threatTarget.Add(nextTarget))
                {
                    Debug.LogFormat("{0} is threatening {1}.", name, nextTarget);
                    nextTarget.AddThreat(gameObject);
                }
            }
        }
    }

    private void ChangeThreatTarget(MoodThreatenable nextTarget)
    {
        ClearCurrentThreats(nextTarget);
        if(nextTarget != null && nextTarget != Threatenable)
        {
            if(_threatTarget.Add(nextTarget))
            {
                Debug.LogFormat("{0} is threatening {1}.", name, nextTarget);
                nextTarget.AddThreat(gameObject);
            }
        }
    }

    private void ClearCurrentThreats(MoodThreatenable except = null)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (threatened == except) continue;

            Debug.LogFormat("{0} is not threatening anymore {1}.", name, threatened);
            threatened.RemoveThreat(gameObject);
        }
        //_threatTarget.Clear();
    }

    private void ClearCurrentThreats(IEnumerable<MoodThreatenable> except)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (except.Contains(threatened) || threatened == null) continue;

            Debug.LogFormat("{0} is not threatening anymore {1}.", name, threatened);
            threatened.RemoveThreat(gameObject);
        }
        //_threatTarget.Clear();
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

    public enum StaminaChangeOrigin
    {
        Action,
        Reaction,
        None
    }

    private float GetCurrentStaminaRecoverValue()
    {
        bool isMoving = IsMoving();
        float value = isMoving ? staminaRecoveryMoving : staminaRecoveryIdle;
        foreach(ActivateableMoodStance stance in AddedStances)
        {
            foreach (IMoodPawnModifierStamina mod in stance.GetAllModifiers<IMoodPawnModifierStamina>()) 
                mod.ModifyStamina(ref value, isMoving);
        }
        return value;
    }

    private bool ShouldRecoverStamina()
    {
        return recoverStaminaWhileUsingSkill || !GetCurrentSkill();
    }

    private void RecoverStamina(float recovery, float timeDelta)
    {
        ChangeStamina(recovery * timeDelta, StaminaChangeOrigin.Action);
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

    public void DepleteStamina(float cost, StaminaChangeOrigin origin)
    {
        ChangeStamina(-cost, origin);
    }

    private void ChangeStamina(float change, StaminaChangeOrigin origin)
    {
        float oldStamina = _stamina;
        _stamina = Mathf.Clamp(_stamina + change, 0f, maxStamina);
        if (oldStamina != _stamina)
        {
            OnChangeStamina?.Invoke(this);
            if(_stamina == 0f)
            {
                switch (origin)
                {
                    case StaminaChangeOrigin.Action:
                        AddFlag(pawnConfiguration?.onStaminaDownByAction);
                        break;
                    case StaminaChangeOrigin.Reaction:
                        AddFlag(pawnConfiguration?.onStaminaDownByReaction);
                        break;
                    case StaminaChangeOrigin.None:
                        break;
                    default:
                        break;
                }
            }
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
        return pawn == this || IsSensing(pawn.SensorTarget);
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

    #region Debug
    [LHH.Unity.Button]
    [ContextMenu("Debug Stances")]
    public void DebugActiveStances()
    {
        foreach(var stance in AllActiveStances)
            Debug.LogFormat("{0} is in {1}", name, stance);
    }


    [LHH.Unity.Button]
    [ContextMenu("Debug Locks")]
    public void DebugLocks()
    {
        Debug.LogFormat("[LOCK] Movement is {0}", _movementLock);
        Debug.LogFormat("[LOCK] Action is {0}", _actionStunLock);
        Debug.LogFormat("[LOCK] Reaction is {0}", _reactionStunLock);
    }
    #endregion


}
