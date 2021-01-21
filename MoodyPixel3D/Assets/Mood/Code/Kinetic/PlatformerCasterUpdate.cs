using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformerCasterUpdate : PlatformerCaster
{
    public override bool NeedExternalCheck => enabled;

    private Rigidbody _body;
    private Rigidbody Body
    {
        get
        {
            if (_body == null) _body = GetComponentInParent<Rigidbody>();
            return _body;
        }
    }

    private void FixedUpdate()
    {
        if(Body.velocity.y != 0)
            Cast();
    }
}
