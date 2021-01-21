using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDrag : AddonBehaviour<Rigidbody>
{
    public float drag = 0f;
    
    [Space()]
    public bool ignoreX = false;
    public bool ignoreY = true;
    public bool ignoreZ = false;
    
    private void FixedUpdate()
    {
        Vector3 newVel = Depreciate(Addon.velocity);
        if (ignoreX) newVel.x = Addon.velocity.x;
        if (ignoreY) newVel.y = Addon.velocity.y;
        if (ignoreZ) newVel.z = Addon.velocity.z;
        Addon.velocity = newVel;
    }

    private Vector3 Depreciate(Vector3 velocity)
    {
        return velocity * (1 - Time.fixedDeltaTime * drag);
    }
}
