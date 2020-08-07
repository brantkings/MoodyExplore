using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlatformerCaster))]
public class KinematicFaller : RigidbodyController
{
    [SerializeField]
    private float _fallMaxVelocity = 54f;
    [SerializeField]
    private float _fallTimeToMaxVelocity = 10f;
    [SerializeField]
    private bool _checkGroundedAlways;
    
    private PlatformerCaster plat;
        
    private float _vel;
    private float _velDelta;
    

    private void Awake()
    {
        plat = GetComponent<PlatformerCaster>();
    }

    private bool IsGrounded()
    {
        if(_checkGroundedAlways)
        {
            return plat.Cast();
        }
        else
        {
            return plat.IsGrounded() == GroundedState.Grounded;
        }
    }
    private float CalculateVelocity(float currentVelocity, ref float delta)
    {
        return Mathf.SmoothDamp(currentVelocity, _fallMaxVelocity, ref delta, _fallTimeToMaxVelocity);
    }


    private void FixedUpdate()
    {
        if(!IsGrounded())
        {
            _vel = CalculateVelocity(_vel, ref _velDelta);

            Body.MovePosition(Body.position + Vector3.down * _vel * Time.fixedDeltaTime);
        }
    }
}
