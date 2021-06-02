using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public partial class KinematicPlatformer
{
    private Vector3 _lerpPosition;
    private Vector3 _currentDirection;

    private Vector3 GetPawnLerpPosition()
    {
        return _lerpPosition;
    }

    private void SetPawnLerpPositionIgnoreGravity(Vector3 set)
    {
        Vector3 diff = set - _lerpPosition;
        AddExactNextFrameMove(diff - KinematicPlatformer.GetGravityForce() * Time.fixedDeltaTime);
        _lerpPosition = set;
    }

    private void SetPawnLerpPosition(Vector3 set)
    {
        if (set.IsNaN()) return;
        Vector3 diff = set - _lerpPosition;
#if UNITY_EDITOR
        if (diff.IsNaN())
            Debug.LogErrorFormat("{0} setting lerp position NaN! {1} - {2} = {3}", name, set, _lerpPosition, diff);
#endif
        AddExactNextFrameMove(diff);
        _lerpPosition = set;
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


    public Tween TweenMoverPositionIgnoreGravity(Vector3 movement, float duration)
    {
        _lerpPosition = Position;
        Tween t = DOTween.To(GetPawnLerpPosition, SetPawnLerpPositionIgnoreGravity, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed);
        return t;
    }

    public Tween TweenMoverPosition(Vector3 movement, float duration)
    {
        _lerpPosition = Position;
        if (duration == 0f)
        {
            SetPawnLerpPosition(Position + movement);
            return null;
        }
        else
        {
            Tween t = DOTween.To(GetPawnLerpPosition, SetPawnLerpPosition, movement, duration).SetId(this).SetRelative(true).SetUpdate(UpdateType.Fixed);
            return t;
        }

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
        else return DOTween.To(GetPawnLerpDirection, SetPawnLerpDirection, directionTo, duration).SetId(this).OnKill(()=> Debug.LogFormat("Killed me {0} {1} [{2}]!", this, directionTo, Time.time));//.OnKill(CallEndMove).OnStart(CallBeginMove);
    }
}
