using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



public class MoodPawn : MonoBehaviour
{
    public KinematicPlatformer mover;

    public delegate void PawnEvent();


    public Vector3 Position
    {
        get
        {
            return mover.Position;
        }
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

    public IEnumerator ExecuteSkill(MoodSkill skill, Vector3 direction)
    {
        yield return StartCoroutine(skill.Execute(this, direction));
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
    #endregion
    
}
