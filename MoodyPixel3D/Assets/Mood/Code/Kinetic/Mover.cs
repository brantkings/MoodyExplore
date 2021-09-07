using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

public class Mover : RigidbodyController , IHorizontalMover {
    
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private Caster _wallCaster;

    private Vector3 _latestSpeed;


    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }

    public void Move(Vector2 direction)
    {
        Vector3 movement = new Vector3(direction.x, 0f, direction.y);

        //Body.MovePosition(Body.position + movement * _speed * Time.deltaTime);
        //Body.velocity += movement * _speed * Time.deltaTime;
        //Body.AddForce(movement * _speed, ForceMode.VelocityChange);
        _latestSpeed = movement * _speed;
    }

    private void FixedUpdate()
    {
        Vector3 movement = _latestSpeed * Time.deltaTime;
        while (_wallCaster.CastLength(movement, movement.magnitude, out RaycastHit hit))
        {
            if (hit.distance > 0f)
            {
                movement = _latestSpeed.normalized * hit.distance;
            }
            else return;
        }

        Body.MovePosition(Body.position + movement);
    }
}
