using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameRadiusAsParentRigidbody : SameRadius<Rigidbody>
{
    public override float GetRadius()
    {
        if (Addon == null) return defaultRadius;
        Bounds b = new Bounds(Addon.centerOfMass, Vector3.zero);
        foreach(Collider c in Addon.GetComponentsInChildren<Collider>())
        {
            b.SetMinMax(Vector3.Min(c.bounds.min, b.min), Vector3.Max(c.bounds.max, b.max));
        }
        Vector3 size = b.max - b.min;
        size = Vector3.ProjectOnPlane(size, Vector3.up);
        return size.magnitude / 2f;
    }


}
