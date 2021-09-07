using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KinematicPlatformerGravity : AddonBehaviour<KinematicPlatformer>, IKinematicPlatformerVelocityGetter
{
    public float groundedGravitySpeed = 0.5f;
    public float finalGravitySpeed = 10f;
    public float timeToMaxSpeed = 1f;
    public Vector3 gravityDirection = -Vector3.up;
    public AnimationCurve easeToMaxSpeed = AnimationCurve.Linear(0f,0f,1f,1f);
    public int priority = 0;

    private float _currentSpeed;

    private void Start()
    {
        gravityDirection = gravityDirection.normalized;
    }

    private void OnEnable()
    {
        Addon.AddVelocitySource(this, priority);
        OnChangedGrounded(Addon.Grounded);
        Addon.Grounded.OnChanged += OnChangedGrounded;
    }

    private void OnDisable()
    {
        Addon.RemoveVelocitySource(this, priority);
        Addon.Grounded.OnChanged += OnChangedGrounded;
    }

    private void OnChangedGrounded(bool change)
    {
        if(change)
        {
            DOTween.Kill(this, true);
            _currentSpeed = groundedGravitySpeed;
        }
        else
        {
            RestartFall();
        }
    }


    private void RestartFall()
    {
        _currentSpeed = groundedGravitySpeed;
        DOTween.Kill(this);
        DOTween.To(GetCurrentSpeed, SetCurrentSpeed, finalGravitySpeed, timeToMaxSpeed).SetEase(easeToMaxSpeed).SetId(this);
    }

    private void SetCurrentSpeed(float s)
    {
        _currentSpeed = s;
    }

    private float GetCurrentSpeed()
    {
        return _currentSpeed;
    }

    public Vector3 GetVelocity()
    {
        /*Debug.LogFormat("[GRAVITY] Getting {0} * {1} = {2} for {3}. Addon is {4}.",
            gravityDirection, _currentSpeed, gravityDirection * _currentSpeed, this, Addon.Grounded? "grounded" : "not grounded");*/
        return gravityDirection * _currentSpeed;
    }
}