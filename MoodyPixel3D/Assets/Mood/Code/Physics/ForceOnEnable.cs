using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceOnEnable : AddonBehaviour<Rigidbody>
{
    public Vector3 forceRelativeToRotation;
    public ForceMode forceMode = ForceMode.Impulse;

    private void OnEnable()
    {
        Addon.AddForce(transform.rotation * forceRelativeToRotation, forceMode);
    }
}
