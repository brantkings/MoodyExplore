using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Code.Animation.Humanoid;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using LHH.Utils;

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
    public Transform toDirect;
    [SerializeField]
    private LookAtIK _lookAtControl;

    [Space()] 
    public float turningTime = 0.1f;
    public float height = 2f;
    public float pawnRadius = 0.5f;

    public float extraRangeBase = 0f;
    
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


    public Vector3 Position => mover.Position;
    
    public Vector3 Direction => toDirect != null? toDirect.forward : mover.transform.forward;


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


    private void Start()
    {
        _stamina = maxStamina;
        OnChangeStamina?.Invoke(this);
        _wasThreatened = IsThreatened();
    }

    private void Update()
    {
        float staminaRecovery = IsMoving() ? staminaRecoveryMoving : staminaRecoveryIdle; 
        RecoverStamina(staminaRecovery, Time.deltaTime);

        Vector3 forward = toDirect.forward;
        if (Vector3.Dot(forward, _directionTarget) < 0f) forward = Quaternion.Euler(0f,10f,0) * forward;
        toDirect.forward = Vector3.SmoothDamp(forward, _directionTarget, ref _directionVel, turningTime, float.MaxValue,
            Time.deltaTime);
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
    
    

    #region Movement
    public event PawnEvent OnBeginMove;
    public event PawnEvent OnEndMove;

    public void SetVelocity(Vector3 velocity)
    {
        mover.SetVelocity(velocity);
        if (velocity.sqrMagnitude > 0.001f)
        {
            _directionTarget = Vector3.ProjectOnPlane(velocity, Vector3.up);
        }
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
        _position = mover.Position;
        return DOTween.To(GetPawnLerpPosition, SetPawnPosition, direction, duration).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove);
    }

    private void CallBeginMove()
    {
        OnBeginMove?.Invoke();
    }

    private void CallEndMove()
    {
        OnEndMove?.Invoke();
    }

    private Vector3 _position;

    private Vector3 GetPawnLerpPosition()
    {
        return _position;
    }

    private void SetPawnPosition(Vector3 set)
    {
        Vector3 diff = set - _position;
        mover.AddExactNextFrameMove(set - _position);
        _position = set;
    }

    public void StartSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack", true);
    }

    public void FinishSkillAnimation(MoodSkill skill)
    {
        animator.SetBool("Attack", false);
    }

    public void SetDirection(Vector3 direction)
    {
        toDirect.forward = Vector3.ProjectOnPlane(direction, Vector3.up);
    }

    public void SetLookAt(Vector3 direction)
    {
        _lookAtControl.LookAt(direction);
    }

    public Quaternion GetDirection()
    {
        return toDirect.rotation;
    }
    #endregion

    #region Threat
    private HashSet<GameObject> _threatList;
    public event DelMoodPawnEvent OnThreatAppear;
    public event DelMoodPawnEvent OnThreatRelief;

    private bool _wasThreatened;
    private Vector3 _threatDirection;

    public void StartThreatening(Vector3 direction)
    {
        _threatDirection = direction;
    }

    public void QuitThreatening()
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

    private void ChangeThreatTarget(MoodPawn target)
    {
        if (_threatTarget == target) return;
        if (_threatTarget != null)
        {
            _threatTarget.RemoveThreat(gameObject);
        }
        if (target != null)
        {
            target.AddThreat(gameObject);
            _threatTarget = target;
        }
    }
    
    public void AddThreat(GameObject origin)
    {
        if(_threatList == null) _threatList = new HashSet<GameObject>();
        _threatList.Add(origin);
        bool isThreatened = IsThreatened();
        if (!_wasThreatened && isThreatened)
            OnThreatAppear?.Invoke(this);
        _wasThreatened = isThreatened;
    }

    public void RemoveThreat(GameObject origin)
    {
        _threatList?.Remove(origin);
        bool isThreatened = IsThreatened();
        if(_wasThreatened && !IsThreatened()) 
            OnThreatRelief?.Invoke(this);
        _wasThreatened = isThreatened;
    }

    public bool IsThreatened()
    {
        if (_threatList != null)
        {
            foreach (GameObject o in _threatList)
            {
                if (o != null) return true;
            }
        }

        return false;
    }
    #endregion
    

    #region Stamina

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
        return GetDirection();
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
