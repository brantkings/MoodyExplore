using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using UnityEngine;

public class OscillatePosition : MonoBehaviour
{
    public Vector3 positionAdd = Vector3.up;

    public float timeBetweenOscillation = 0.25f;

    public bool unscaled;
    
    private float _time;
    private bool _on;

    private Vector3 _posInitial;

    private void Start()
    {
        _posInitial = transform.localPosition;
    }

    public void Update()
    {
        _time -= unscaled? Time.unscaledDeltaTime : Time.deltaTime;
        if (_time <= 0f)
        {
            _time += timeBetweenOscillation;
            SetPosition(!_on);
        }
    }

    private void SetPosition(bool set)
    {
        _on = set;
        transform.localPosition = _on ? _posInitial + positionAdd : _posInitial;
    }
    
}
