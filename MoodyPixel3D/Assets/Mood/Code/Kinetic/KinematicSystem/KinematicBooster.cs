using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KinematicBoost : IKinematicPlatformerFrameVelocityGetter
{
    [System.Serializable]
    public struct Data
    {
        public Vector3 relativeBoost;
        public float duration;
        public Ease ease;

        public bool IsValid()
        {
            return relativeBoost != Vector3.zero;
        }
    }


    private Vector3 currentPosition;
    private Vector3 reportedPosition;
    private Tween _tween;

    public Tween Boost(Data data, Transform origin)
    {
        if (data.IsValid())
            return Boost(origin.TransformVector(data.relativeBoost), data.duration, data.ease);
        else return null;
    }

    public Tween Boost(Vector3 toRelativePosition, float duration, Ease ease)
    {
        return Boost(toRelativePosition, duration).SetEase(ease);
    }

    public Tween Boost(Vector3 toRelativePosition, float duration, AnimationCurve ease)
    {
        return Boost(toRelativePosition, duration).SetEase(ease);
    }

    public Tween Boost(Vector3 toRelativePosition, float duration)
    {
        _tween.KillIfActive();
        currentPosition = Vector3.zero;
        reportedPosition = Vector3.zero;
        _tween = DOTween.To(() => currentPosition, (x) => currentPosition = x, toRelativePosition, duration).Pause();
        return _tween;
    }

    public void InterruptBoost()
    {
        _tween.KillIfActive();
        _tween = null;
    }

    public void Attach(KinematicPlatformer plat)
    {
        plat.AddVelocitySource(this);
    }

    public void Remove(KinematicPlatformer plat)
    {
        plat.RemoveVelocitySource(this);
    }

    public Vector3 GetFrameVelocity(float deltaTime)
    {
        if (_tween != null && !_tween.IsComplete())
        {
            _tween.Goto(_tween.position + deltaTime, false);
            if (_tween.fullPosition >= (_tween.Duration() + _tween.Delay()))
            {
                Debug.LogFormat("Gonna kill tween!");
                //_tween.Kill(true);
            }
            Vector3 move = currentPosition - reportedPosition;
            reportedPosition = currentPosition;
            Debug.LogFormat("Doing move {0} in position {1} -> {2}", move.ToString("F4"), _tween.fullPosition, currentPosition);
            return move;
        }
        else return Vector3.zero;
    }
}

[RequireComponent(typeof(KinematicPlatformer))]
public class KinematicBooster : AddonBehaviour<KinematicPlatformer>
{
    public KinematicBoost boost = new KinematicBoost();

    public Vector3 testBoost;
    public float testDuration = 1f;

    private void OnEnable()
    {
        boost.Attach(Addon);
    }

    private void OnDisable()
    {
        boost.Remove(Addon);
    }

    public void Boost(KinematicBoost.Data data)
    {
        boost.Boost(data, transform);
    }

    [ContextMenu("Do test boost")]
    public void TestBoost()
    { 
        boost.Boost(testBoost, testDuration);
    }
}
