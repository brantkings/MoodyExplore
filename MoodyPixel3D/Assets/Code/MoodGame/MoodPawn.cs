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

public interface IMoodPawnPeeker
{
    void SetTarget(MoodPawn pawn);
    void UnsetTarget(MoodPawn pawn);
}

public class MoodPawn : MonoBehaviour
{
    public delegate void DelMoodPawnEvent(MoodPawn pawn);
    public delegate void DelMoodPawnSkillEvent(MoodSkill skill, Vector3 direction);
    public delegate void DelMoodPawnUndirectedSkillEvent(MoodSkill skill);

    public event DelMoodPawnEvent OnChangeStamina;
    public event DelMoodPawnSkillEvent OnUseSKill;
    public event DelMoodPawnUndirectedSkillEvent OnInterruptSkill;
    
    public KinematicPlatformer mover;
    public Animator animator;
    private LookAtIK _lookAtControl;

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

    private Vector3 _directionTarget;
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


    private void Start()
    {
        _stamina = maxStamina;
        OnChangeStamina?.Invoke(this);
        _wasThreatened = IsThreatened();
    }

    private void OnGUI() 
    {
        if(damageTeam == DamageTeam.Neutral)
        {
            string stances = "";
            foreach(var st in Stances)
            {
                stances += st.name;
                stances += " ";
            }
            GUI.Label(new Rect(50,Screen.height - 100, 100, 100), string.Format("Current stances {0}: {1}", name, stances));
        }
    }

    private void Update()
    {
        RecoverStamina(GetCurrentStaminaRecoverValue(), Time.deltaTime);

        //Direction = _directionTarget;
        Vector3 forward = mover.transform.forward;
        if (Vector3.Dot(forward, _directionTarget) < 0f) forward = Quaternion.Euler(0f,10f,0) * forward;
        Direction = Vector3.SmoothDamp(forward, _directionTarget, ref _directionVel, turningTime, float.MaxValue, Time.deltaTime);
    }
    
    private void FixedUpdate()
    {
        ThreatFixedUpdate(_threatDirection);
    }
    
    public void UsedSkill(MoodSkill skill, Vector3 direction)
    {
        OnUseSKill?.Invoke(skill, direction);
    }

    public void InterruptedSkill(MoodSkill skill)
    {
        OnInterruptSkill?.Invoke(skill);
    }
    
    #region Skills
    private MoodSkill _currentSkill;
    public void MarkUsingSkill(MoodSkill skill)
    {
        _currentSkill = skill;
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

    private Vector3 _targetVelocity;
    private Tween _movementTween;

    private void SolveFinalVelocity()
    {
        if (_movementTween != null && _movementTween.IsActive())
        {
            mover.SetVelocity(Vector3.zero);
        }
        else
        {
            if (cantMoveWhileThreatened && IsThreatened())
            {
                mover.SetVelocity(Vector3.zero);
            }
            else
            {
                mover.SetVelocity(_targetVelocity);
                if (_targetVelocity.sqrMagnitude > 0.001f)
                {
                    _directionTarget = Vector3.ProjectOnPlane(_targetVelocity, Vector3.up);
                }
            }
            
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
        _targetVelocity = velocity;
        SolveFinalVelocity();
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
        SolveFinalVelocity();
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

    public void SetHorizontalDirection(Vector3 direction)
    {
        _directionTarget = direction;
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
    #endregion

    #region Threat
    struct ThreatStruct
    {
        public GameObject threatObject;
        public SensorTarget sensorTarget;

        public ThreatStruct(GameObject obj)
        {
            threatObject = obj;
            sensorTarget = obj.GetComponentInChildren<SensorTarget>();
        }
    }

    private HashSet<ThreatStruct> _threatList;
    public event DelMoodPawnEvent OnThreatAppear;
    public event DelMoodPawnEvent OnThreatRelief;

    private bool _wasThreatened;
    private Vector3 _threatDirection;

    public void StartThreatening(Vector3 direction)
    {
        _threatDirection = direction;
    }

    public void StopThreatening()
    {
        _threatDirection = Vector3.zero;
    }
    
    private MoodPawn _threatTarget;
    
    private void ThreatFixedUpdate(Vector3 threatDirection)
    {
        if (threatDirection == Vector3.zero)
        {
            ChangeThreatTarget(null);
            return;
        }
        ChangeThreatTarget(FindTarget(threatDirection, threatDirection.magnitude)?.GetComponentInParent<MoodPawn>());
    }

    private void ChangeThreatTarget(MoodPawn nextTarget)
    {
        if (_threatTarget == nextTarget) return;
        if (_threatTarget != null)
        {
            _threatTarget.RemoveThreat(gameObject);
            _threatTarget = null;
        }
        if (nextTarget != null)
        {
            nextTarget.AddThreat(gameObject);
            _threatTarget = nextTarget;
        }
    }

    public void AddThreat(GameObject origin)
    {
        Debug.LogFormat("Add threat {0} to {1}", origin, this);
        if(_threatList == null) _threatList = new HashSet<ThreatStruct>();
        _wasThreatened = IsThreatened();
        _threatList.Add(new ThreatStruct(origin));
        bool isThreatened = IsThreatened();
        if (!_wasThreatened && isThreatened)
        {
            OnThreatAppear?.Invoke(this);
            SolveFinalVelocity();
        }
        _wasThreatened = isThreatened;
    }

    public void RemoveThreat(GameObject origin)
    {
        Debug.LogFormat("Remove threat {0} to {1}", origin, this);
        _wasThreatened = IsThreatened();
        _threatList?.RemoveWhere((threatStruct) => threatStruct.threatObject == origin);
        bool isThreatened = IsThreatened();
        if (_wasThreatened && !IsThreatened())
        {
            OnThreatRelief?.Invoke(this);
            SolveFinalVelocity();
        }
        _wasThreatened = isThreatened;
    }

    public bool IsThreatened()
    {
        if (_threatList != null)
        {
            foreach (ThreatStruct threat in _threatList)
            {
                if (threat.threatObject != null && 
                    (threat.sensorTarget == null || (sensorGroup.IsSensingTarget(threat.sensorTarget)))) 
                    return true;
            }
        }

        return false;
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
