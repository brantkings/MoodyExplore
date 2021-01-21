using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Detector : MonoBehaviour, Detector.IDetector
{
    public delegate void DelDetectorEvent();
    public delegate void DelDetectorEventChange(Detector d, bool detecting);
    public event DelDetectorEvent OnDetect;
    public event DelDetectorEvent OnUndetect;
    public event DelDetectorEventChange OnChangeDetect;

    public interface IDetector
    {
        bool IsDetecting
        {
            get;
        }
        event DelDetectorEvent OnDetect;
        event DelDetectorEvent OnUndetect;
        event DelDetectorEventChange OnChangeDetect;
    }

    Transform _target;

    [SerializeField]
    private Transform _customCenter;

    [ReadOnly]
    [SerializeField]
    private bool _detecting;

    [SerializeField]
    private bool _lockDetecting;
    [SerializeField]
    private float _minTimeDetecting;
    [SerializeField]
    private float _minTimeUndetecting;

    private float _lastTimeChanged;

    public bool IsDetecting
    {
        get
        {
            return _detecting;
        }
    }

    protected void UpdateTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    public virtual Vector3 GetCenter()
    {
        if (_customCenter != null) return _customCenter.position;
        else return transform.position;
    }

    public virtual Vector3? GetTargetPosition()
    {
        if (_target != null) return _target.position;
        else return null;
    }

    public Vector3? GetDistanceToTarget()
    {
        return GetTargetPosition() - GetCenter();
    }

    protected void TryUpdateDetecting(bool newState)
    {
        //Debug.LogFormat("Trying {0}, is {1} last time was {2} time needed is {3}", newState, _detecting, _lastTimeChanged, MinTimeDetecting(_detecting));
        if (newState == _detecting) return;
        if (_detecting && _lockDetecting) return;
        if (Time.time - _lastTimeChanged < MinTimeDetecting(_detecting)) return;

        UpdateDetecting(newState);
    }

    public void UpdateDetecting(bool newState)
    {
        bool different = _detecting != newState;
        _detecting = newState;
        _lastTimeChanged = Time.time;

        if(different)
        {
            if (OnChangeDetect != null) OnChangeDetect(this, _detecting);
            if (_detecting && OnDetect != null) OnDetect();
            if (!_detecting && OnUndetect != null) OnUndetect();
        }
    }
    
    private float MinTimeDetecting(bool wasDetecting)
    {
        if (wasDetecting) return _minTimeDetecting;
        else return _minTimeUndetecting;
    }
    
}
