using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public interface IMoodPawnPeeker
{
    void SetTarget(MoodPawn pawn);
    void UnsetTarget(MoodPawn pawn);
}

public class MoodPawn : MonoBehaviour
{
    public delegate void DelMoodPawnEvent(MoodPawn pawn);
    public delegate void DelMoodPawnSkillEvent(MoodSkill skill, Vector3 direction);

    public event DelMoodPawnEvent OnDepleteStamina;
    
    public KinematicPlatformer mover;
    public Animator animator;
    public Transform toDirect;

    public float maxStamina;
    private float _stamina;
    public bool infiniteStamina;

    [SerializeField]
    private DamageTeam damageTeam = DamageTeam.Neutral;

    public delegate void PawnEvent();


    public Vector3 Position => mover.Position;
    
    public Vector3 Direction => toDirect != null? toDirect.forward : mover.transform.forward;

    public DamageTeam DamageTeam => damageTeam;

    private void Start()
    {
        _stamina = maxStamina;
        OnDepleteStamina?.Invoke(this);
    }

    #region Movement
    public event PawnEvent OnBeginMove;
    public event PawnEvent OnEndMove;
    public Tween Move(Vector3 direction, float duration, AnimationCurve curve)
    {
        return TweenMoverPosition(direction, duration).SetEase(curve);
    }
    
    public Tween Move(Vector3 direction, float duration, Ease ease)
    {
        return TweenMoverPosition(direction, duration).SetEase(ease);
    }

    private Tween TweenMoverPosition(Vector3 direction, float duration)
    {
        CallBeginMove();
        _position = mover.Position;
        return DOTween.To(GetPawnPosition, SetPawnPosition, direction, duration).SetRelative(true).SetUpdate(UpdateType.Fixed).OnKill(CallEndMove);
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

    private Vector3 GetPawnPosition()
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
        _stamina = Mathf.Clamp(_stamina - cost, 0f, maxStamina);
        OnDepleteStamina?.Invoke(this);
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
    
}
