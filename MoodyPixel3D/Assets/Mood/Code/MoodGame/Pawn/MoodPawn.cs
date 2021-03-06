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
    public delegate void DelMoodPawnDamageEvent(MoodPawn pawn, DamageInfo info);
    public delegate void DelMoodPawnSkillEvent(MoodPawn pawn, MoodSkill skill, Vector3 direction);
    public delegate void DelMoodPawnItemEvent(MoodPawn pawn, MoodItem item);
    public delegate void DelMoodPawnSwingEvent(MoodSwing.MoodSwingBuildData swing, Vector3 direction);
    public delegate void DelMoodPawnUndirectedSkillEvent(MoodPawn pawn, MoodSkill skill);

    public static event DelMoodPawnDamageEvent OnAnyMoodPawnDie;

    public event DelMoodPawnEvent OnChangeStamina;

    public event DelMoodPawnSkillEvent OnBeforeSkillUse;
    public event DelMoodPawnSwingEvent OnBeforeSwinging;
    public event DelMoodPawnSkillEvent OnUseSkill;
    public event DelMoodPawnItemEvent OnUseItem;
    public event DelMoodPawnDamageEvent OnPawnDeath;
    public event DelMoodPawnUndirectedSkillEvent OnEndSkill;
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
    [SerializeField]
    private MoodInventory _inventory;
    [Space]
    [SerializeField]
    private GameObject toDestroyOnDeath;

    [SerializeField]
    private MoodPawnStanceConfiguration pawnConfiguration;
    [SerializeField]
    private MoodPreReaction[] inherentReactions;
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

    public MoodInventory Inventory
    {
        get
        {
            return _inventory;
        }
    }

    [Space()]
    public float timeToMaxVelocity;
    public float timeToZeroVelocity = 0f;
    public float snapToTargetSpeedDelta = 0.1f;
    public float angleToBeAbleToAccelerate = 90f;
    public TimeBeatManager.BeatQuantity turningTime = 1;
    public float height = 2f;
    public float pawnRadius = 0.5f;

    public float extraRangeBase = 0f;
    public bool cantMoveWhileThreatened = true;
    
    [Space()]
    [UnityEngine.Serialization.FormerlySerializedAs("_maxStamina")]
    public MoodParameter<float> _maxStamina = 1;
    private float _stamina;
    public bool infiniteStamina;
    public bool recoverStaminaWhileUsingSkill;
    public TimeBeatManager.BeatQuantity staminaRecoveryIdlePerSecond = 8;
    public TimeBeatManager.BeatQuantity staminaRecoveryMovingPerSecond = 8;
    private Vector3 _damageAnimation;

    [UnityEngine.Serialization.FormerlySerializedAs("_handPosition")]
    public Transform _instantiateProjectilePosition;

    [SerializeField]
    private DamageTeam damageTeam = DamageTeam.Neutral;

    public delegate void PawnEvent();

    private HashSet<ActivateableMoodStance> _currentActivateableStances;


    public Vector3 Position => mover.Position;

    public Vector3 Up => mover.transform.up;

    public Vector3 Direction
    {
        get => mover.Direction;
        set
        {
            if(value != Vector3.zero)
                mover.Direction = value.normalized;
        }
        
    }

    public Vector3 MovingDirection
    {
        get
        {
            return mover.LatestNonZeroVelocity.normalized;
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
        if(animator != null)
        {
            _lookAtControl = animator.GetComponent<LookAtIK>();
        }
        _maxStamina.OnChange += OnChangeMaxStamina;
        
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
        _stamina = _maxStamina;
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

    public void PrepareForSwing(MoodSwing.MoodSwingBuildData swing, Vector3 direction)
    {
        OnBeforeSwinging?.Invoke(swing, direction);
    }

    public void ShowSwing(MoodSwing.MoodSwingBuildData data, Vector3 direction)
    {
        if (_attackFeedback != null)
            _attackFeedback.DoFeedback(data, direction);
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
        Debug.LogFormat("[PAWN] {0} takes damage with info {1}.", name , info);
        if (info.damage > 0 && pawnConfiguration != null) 
            AddFlag(pawnConfiguration.onDamage);
        HandleDamageInfo(info, health);
    }

    private bool ShouldStaggerAnimation(DamageInfo info)
    {
        return info.shouldStaggerAnimation;
    }

    public void HandleDamageInfo(DamageInfo info, Health health)
    {
        
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
        Debug.LogFormat("{0} perished.", this);
        OnAnyMoodPawnDie?.Invoke(this, info);
        OnPawnDeath?.Invoke(this, info);
        if (toDestroyOnDeath != null) Destroy(toDestroyOnDeath);
    }
    #endregion

    #region Items
    public void UseItem(MoodItem item, MoodSkill skill)
    {
        item.OnUse(this);
        Inventory.UseItem(item);
        OnUseItem?.Invoke(this, item);
    }
    #endregion

    #region Skills
    private float _currentSkillBeginTimestamp;
    private float _currentSkillUseTimestamp;
    private MoodSkill _currentSkill;
    private Coroutine _currentSkillRoutine;
    private int _currentPlugoutPriority;
    private Vector3? _currentSkillUsePosition;
    private Vector3 _currentSkillOriginDirection;
    private Vector3 _currentSkillOriginPosition;

    public Tween DelayedAction(TweenCallback act, float delay)
    {
        return DOVirtual.DelayedCall(delay, act).SetId(this);
    }


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
        MarkUsingSkill(skill, skillDirection);
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
            UnmarkUsingSkill(skill);
            OnInterruptSkill?.Invoke(this, skill);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogErrorFormat("[PAWN] Tried interrupting skill {0} for pawn {1} but couldn't!", skill.name, name);
        }
#endif
    }

    public float GetTimeElapsedSinceBeganCurrentSkill()
    {
        return Time.time - _currentSkillBeginTimestamp;
    }

    public float GetTimeElapsedSinceUsedCurrentSkill()
    {
        return Time.time - _currentSkillUseTimestamp;
    }

    public bool UsedCurrentSkill()
    {
        return _currentSkillUseTimestamp > _currentSkillBeginTimestamp;
    }

    public Vector3 GetSkillPreviewOriginPosition()
    {
        Vector3? usePosition = GetLatestSkillUsePosition();
        return usePosition.HasValue ? usePosition.Value : GetCurrentSkillOriginPosition();
    }

    public Vector3 GetCurrentSkillOriginPosition()
    {
        if (IsExecutingSkill()) return _currentSkillOriginPosition;
        else return Position;
    }

    public Vector3 GetCurrentSkillOriginDirection()
    {
        if (IsExecutingSkill()) return _currentSkillOriginDirection;
        else return Direction;
    }

    public Vector3? GetLatestSkillUsePosition()
    {
        return _currentSkillUsePosition;
    }

    public void MarkUsingSkill(MoodSkill skill, Vector3 direction)
    {
        //Debug.LogFormat("{0} mark using skill {1}", this.name, skill?.name);
        _currentSkill = skill;
        _currentSkillBeginTimestamp = Time.time;
        _currentSkillOriginDirection = Direction;
        _currentSkillOriginPosition = Position;
        _currentSkillUsePosition = null;
        OnBeforeSkillUse?.Invoke(this, skill, direction);
    }

    public void UnmarkUsingSkill(MoodSkill skill)
    {
        if (_currentSkill == skill)
        {
            //Debug.LogFormat("{0} unmark using skill {1}", this.name, skill?.name);
            _currentSkill = null;
            _currentSkillUsePosition = null;
            SetPlugoutPriority(0);
            OnEndSkill?.Invoke(this, skill);
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
        Debug.LogFormat("{0} executes {1}! ({2})", name, skill.name, Time.frameCount);
        _currentSkillUseTimestamp = Time.time;
        _currentSkillUsePosition = Position;
        OnUseSkill?.Invoke(this, skill, direction);
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

    private void AddStancesSemaphore(int add)
    {
        _stancesSemaphore += add;
#if UNITY_EDITOR
        if (_stancesSemaphore > 2) Debug.LogWarningFormat("[SEM] Semaphore of {0} is accumulating {1}!", name, _stancesSemaphore);
#endif
        //Debug.LogFormat("Adding {0} to semaphore: {1}", add, _stancesSemaphore);
        if (add < 0 && !IsRunningThroughStances()) DealWithTempStances();
    }

    private void DealWithTempStances()
    {
        foreach (var t in _postponedModifications)
        {
            Debug.LogFormat("{0} is {1} stance {2} after looping.", name, t.Item1? "adding": "removing", t.Item2);
            if (t.Item1 == true) AddStance(t.Item2);
            else RemoveStance(t.Item2);
        }
        _postponedModifications.Clear();
    }

    private HashSet<ActivateableMoodStance> AddedStances
    {
        get
        {
            if(_currentActivateableStances == null) _currentActivateableStances = new HashSet<ActivateableMoodStance>();
            return _currentActivateableStances;
        }
    }

    private IEnumerable<ActivateableMoodStance> GetSafeAddedStances()
    {
        AddStancesSemaphore(1);
        foreach (var st in AddedStances) yield return st;
        AddStancesSemaphore(-1);
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
            foreach (ActivateableMoodStance stance in GetSafeAddedStances()) yield return stance;
            foreach (ConditionalMoodStance stance in GetActiveConditionalMoodStances()) yield return stance;
        }
    }

    public IEnumerable<IMoodReaction<T>> GetActiveReactions<T>()
    {
        return GetActiveReactions().OfType<IMoodReaction<T>>();
    }


    public IEnumerable<MoodReaction> GetActiveReactions()
    {
        if (IsStunned(StunType.Reaction)) 
            yield break;

        foreach(MoodStance stance in AllActiveStances)
        {
            foreach(MoodReaction react in stance.GetReactions())
            {
                yield return react;
            }
        }
        if (pawnConfiguration != null)
        {
            foreach (MoodReaction react in pawnConfiguration.GetReactions()) yield return react;
        }
        if (inherentReactions != null)
        {
            foreach (MoodReaction react in inherentReactions) yield return react;
        }
    }

    private LinkedList<System.Tuple<bool, ActivateableMoodStance>> _postponedModifications = new LinkedList<Tuple<bool, ActivateableMoodStance>>();

    public enum StanceModificationResult //I use the int values of this
    {
        No = 0,
        Postponed,
        Yes
    }

    public StanceModificationResult AddStance(ActivateableMoodStance stance)
    {
        if(IsRunningThroughStances())
        {
            _postponedModifications.AddLast(new Tuple<bool, ActivateableMoodStance>(true, stance));
            Debug.LogFormat("[STANCE] {0} is going to try to add {1} later.", name, stance.name);
            return StanceModificationResult.Postponed;
        }
        bool ok = AddedStances.Add(stance);
        if (ok)
        {
            Debug.LogFormat("[STANCE] {0} added stance {1}", name, stance.name);
            stance.ApplyStance(this, true);
            return StanceModificationResult.Yes;
        }
        return StanceModificationResult.No;
    }

    public StanceModificationResult RemoveStance(ActivateableMoodStance stance)
    {
        if (IsRunningThroughStances())
        {
            _postponedModifications.AddLast(new Tuple<bool, ActivateableMoodStance>(false, stance));
            return StanceModificationResult.Postponed;
        }
        bool ok = AddedStances.Remove(stance);
        if (ok)
        {
            Debug.LogFormat("[STANCE] {0} removed stance {1}", name, stance.name);
            stance.ApplyStance(this, false);
            return StanceModificationResult.Yes;
        }
        return StanceModificationResult.No;
    }

    public StanceModificationResult AddFlag(MoodEffectFlag flag)
    {
        if (flag == null)
            return StanceModificationResult.No;
        StanceModificationResult changedAnyFlag = 0;
        foreach (var stance in AllFlaggeableStances().Where(x => x.HasFlagToActivate(flag)))
        {
            Debug.LogFormat("{0} added stance {1} due to flag {2}.", this.name, stance, flag.name);
            changedAnyFlag = MergeStanceModResult(AddStance(stance), changedAnyFlag);
        }
        foreach (var stance in GetSafeAddedStances().Where(x => x.HasFlagToDeactivate(flag)))
        {
            RemoveStance(stance);
        }
        return changedAnyFlag;
    }


    public StanceModificationResult AddFlags(IEnumerable<MoodEffectFlag> flags)
    {
        if (flags != null)
        {
            StanceModificationResult didIt = 0;
            foreach (var flag in flags)
            {
                didIt = MergeStanceModResult(didIt, AddFlag(flag));
            }
            return didIt;
        }
        else return StanceModificationResult.No;
    }

    private StanceModificationResult MergeStanceModResult(StanceModificationResult r1, StanceModificationResult r2)
    {
        return (StanceModificationResult)Mathf.Max((int)r1, (int)r2);
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


    public StanceModificationResult ToggleStance(ActivateableMoodStance stance)
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
        AddStancesSemaphore(1);
        foreach(MoodStance stance in AddedStances)
        {
            if (!stance.IsNeutralStance())
            {
                AddStancesSemaphore(-1);
                return false;
            }
        }
        AddStancesSemaphore(-1);
        foreach (MoodStance stance in GetActiveConditionalMoodStances())
        {
            if (!stance.IsNeutralStance())
            {
                return false;
            }
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
    private Tween _currentRotationDash;
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

    private ReactionInfo MakeReactionInfo(Vector3 velocity, GameObject origin, Vector3 normal, bool isGuilty)
    {
        ReactionInfo info = new ReactionInfo(origin, GetKnockbackForce(velocity, isGuilty, out float duration), duration).SetNormal(normal);
        return info;
    }

    private void HandleBumpInfo(ReactionInfo bumpInfo)
    {
        Debug.LogFormat(bumpInfo.origin, "[BUMP] {0} bumped on {1}. ({2})", name, bumpInfo.origin?.name, bumpInfo);
        foreach (IMoodReaction<ReactionInfo> react in GetActiveReactions<ReactionInfo>())
        {
            if (react.CanReact(bumpInfo, this))
            {
                react.React(ref bumpInfo, this);
            }
        }
        
    }

    private void Bump(Vector3 currentVelocity)
    {
        Collider col = mover.WhatIsInThere(currentVelocity.normalized * 0.25f, KinematicPlatformer.CasterClass.Side, out RaycastHit hit);
        if(col != null)
        {
            col.GetComponentInParent<IBumpeable>()?.OnBumped(this.gameObject, currentVelocity, hit.normal);
        }
        HandleBumpInfo(MakeReactionInfo(currentVelocity, col?.gameObject, hit.normal, true));
    }

    public void OnBumped(GameObject origin, Vector3 velocity, Vector3 normal)
    {
        ReactionInfo info = MakeReactionInfo(velocity, origin, normal, false);
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

    public void RotateDash(float angle, float duration, Ease ease = Ease.OutQuad)
    {
        CancelCurrentRotationDash();
        _currentRotationDash = TweenMoverDirection(angle, duration)?.SetEase(ease);
    }

    public void RotateDash(Vector3 dir, float duration, Ease ease = Ease.OutQuad)
    {
        CancelCurrentRotationDash();
        _currentRotationDash = TweenMoverDirection(dir, duration)?.SetEase(ease);
    }

    public Tween TweenMoverDirection(float angleAdd, float duration)
    {
        Vector3 directionAdd = Quaternion.Euler(0f, angleAdd, 0f) * Direction;
        return TweenMoverDirection(directionAdd, duration);
    }

    public Tween TweenMoverDirection(Vector3 directionTo, float duration)
    {
        Debug.LogFormat("{0} is rotating to direction {1}, {2} [{3}]", this, directionTo, duration, Time.time);
        if (duration <= 0f)
        {
            SetPawnLerpDirection(directionTo);
            return null;
        }
        else return DOTween.To(GetPawnLerpDirection, SetPawnLerpDirection, directionTo, duration).SetId(this).OnKill(() => Debug.LogFormat("Killed me {0} {1} [{2}]!", this, directionTo, Time.time));//.OnKill(CallEndMove).OnStart(CallBeginMove);
    }

    private Vector3 GetPawnLerpDirection()
    {
        return _currentDirection;
    }

    private void SetPawnLerpDirection(Vector3 set)
    {
        _currentDirection = set;
        Direction = _currentDirection;
    }

    public void Dash(Vector3 direction, float duration, AnimationCurve curve)
    {
        CancelCurrentDash();
        _currentDash = mover.TweenMoverPosition(direction, duration)?.SetEase(curve).OnKill(CallEndMove).OnStart(CallBeginMove).OnComplete(CallCompleteMove);
    }
    
    public void Dash(Vector3 direction, float duration, Ease ease)
    {
        CancelCurrentDash();
        _currentDash = mover.TweenMoverPosition(direction, duration)?.SetEase(ease).OnKill(CallEndMove).OnStart(CallBeginMove).OnComplete(CallCompleteMove);
    }

    public void CancelCurrentDash()
    {
        if (_currentDash != null) _currentDash.KillIfActive();
    }
    public void CancelCurrentRotationDash()
    {
        if (_currentRotationDash != null) _currentRotationDash.KillIfActive();
    }

    private Tween TweenFakeHeight(float height, float duration, Ease ease)
    {
        _currentFakeHeightHop.CompleteIfActive();
        _currentFakeHeightHop = null;

        _currentFakeHeightHop = DOTween.To(GetPawnFakeHeight, SetPawnFakeHeight, height, duration).SetEase(ease);
        return _currentFakeHeightHop;
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

    

    private float GetPawnFakeHeight()
    {
        return animator.transform.localPosition.y;
    }

    private void SetPawnFakeHeight(float h)
    {
        Vector3 animPos = animator.transform.localPosition;
        animPos.y = h;
        animator.transform.localPosition = animPos;
    }

    private void UpdateAnimation()
    {

    }

    private float timeStampAttack;
    private string lastAttack;

    public enum AnimationPhase
    {
        None = 0,
        PreAttack = 1,
        PostAttack = 2
    }

    public void SetAttackSkillAnimation(string str, AnimationPhase phase)
    {
        lastAttack = str;
        timeStampAttack = Time.time;

        animator.SetInteger(str, (int) phase);
    }

    public void FinishSkillAnimation(string str)
    {
        animator.SetInteger(str, (int)AnimationPhase.None);
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
        if(animator != null)
        {
            animator.SetFloat("DamageX", direction.x);
            animator.SetFloat("DamageZ", direction.z);
        }
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
        _lookAtControl?.LookAt(direction);
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
    private MoodSwing.MoodSwingBuildData? _swingThreat;

    public void StartThreatening(Vector3 direction, MoodSwing.MoodSwingBuildData data)
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
        if(_swingThreat.HasValue)
        {
            ChangeThreatTargets(from result in _swingThreat.Value.TryHitMerged(Position, GetRotation(), MoodGameManager.Instance.GetPawnBodyLayer()) select result.collider.GetComponentInParent<MoodThreatenable>());
        }
        else
        {
            ChangeThreatTarget(FindTarget(threatDirection, threatDirection.magnitude)?.GetComponentInParent<MoodThreatenable>());
        }
    }

    private HashSet<MoodThreatenable> _threatTarget = new HashSet<MoodThreatenable>();
    private void ChangeThreatTargets(IEnumerable<MoodThreatenable> targets)
    {
        ClearCurrentThreats(targets, true);
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
        ClearCurrentThreats(nextTarget, true);
        if(nextTarget != null && nextTarget != Threatenable)
        {
            if(_threatTarget.Add(nextTarget))
            {
                if(nextTarget.AddThreat(gameObject))
                {
                    Debug.LogFormat("{0} is threatening {1}.", name, nextTarget);
                }
            }
        }
    }

    private void ClearCurrentThreats(MoodThreatenable except = null, bool clearList = true)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (threatened == except) continue;

            if(threatened.RemoveThreat(gameObject))
            {
                Debug.LogFormat("{0} is not threatening anymore {1}.", name, threatened);
            }
        }
        if(clearList) _threatTarget.Clear();
    }

    private void ClearCurrentThreats(IEnumerable<MoodThreatenable> except, bool clearList = true)
    {
        foreach (MoodThreatenable threatened in _threatTarget)
        {
            if (except.Contains(threatened) || threatened == null) continue;

            if(threatened.RemoveThreat(gameObject))
            {
                Debug.LogFormat("{0} is not threatening anymore {1}.", name, threatened);
            }
        }
        if(clearList) _threatTarget.Clear();
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
        float value = isMoving ? staminaRecoveryMovingPerSecond : staminaRecoveryIdlePerSecond;
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
        _stamina = Mathf.Clamp(_stamina + change, 0f, _maxStamina);
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
        return _maxStamina;
    }

    private void OnChangeMaxStamina(float before, float after)
    {
        //Debug.LogFormat("Change stamina yay {0} {1} {2}", before, after, this);
        if (before != after) OnChangeStamina?.Invoke(this);
    }


    #endregion

    #region Places

    public Vector3 GetInstantiatePlace()
    {
        return _instantiateProjectilePosition != null ? _instantiateProjectilePosition.position : ObjectTransform.position;
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
        if(threat != null)
        {
            SensorTarget threatTarget = threat.GetComponentInParent<SensorTarget>();
            if (threatTarget != null) return IsSensing(threatTarget);
            MoodPawn pawn = threat.GetComponentInParent<IMoodPawnBelonger>()?.GetMoodPawnOwner();
            if (pawn != null)
            {
                return IsSensing(pawn);
            }
        }
        return false;
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
    public Transform FindTarget(Vector3 offset, Vector3 direction, MoodSwing.MoodSwingBuildData swing, LayerMask target)
    {
        return swing.TryHitGetBest(Position + offset, GetRotation(), target, direction)?.collider.transform;
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
    [ContextMenu("Debug Possible Reactions")]
    public void DebugActiveReactions()
    {
        foreach (var reaction in GetActiveReactions())
                Debug.LogFormat("{0} can react with {1}", name, reaction);
    }

    [LHH.Unity.Button]
    [ContextMenu("Debug Possible Reactions on stances")]
    public void DebugActiveReactionsStances()
    {
        foreach (var stance in AllActiveStances)
            foreach(var reaction in stance.GetReactions())
                Debug.LogFormat("{0} can react with {1} because {2}", name, reaction, stance);

        foreach(var reaction in pawnConfiguration?.GetReactions())
            Debug.LogFormat("{0} can react with {1} because configuration {2}", name, reaction, pawnConfiguration);


        foreach (var reaction in inherentReactions)
            Debug.LogFormat("{0} can react with {1} because inherent reactions", name, reaction);
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
