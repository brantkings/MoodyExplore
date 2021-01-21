using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Utils;

public class PlatformerCasterOnCollision : PlatformerCaster
{
    int _collisions;

    public override bool NeedExternalCheck => false;

    private void OnCollisionEnter(Collision collision)
    {
        if(IsGroundCollision(collision))
        {
            Cast();
        }
        _collisions++;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGroundCollision(collision))
        {
            Cast();
        }
        _collisions--;
    }

    private bool IsGroundCollision(Collision collision)
    {
        return this.GroundLayer.Contains(collision.collider.gameObject.layer);
    }
}
