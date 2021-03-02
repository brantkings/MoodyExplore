using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameRadiusAsParentCollider : SameRadius<Collider>
{
    public override float GetRadius()
    {
        if (Addon == null) return defaultRadius;
        Bounds b = Addon.bounds;
        Vector3 size = b.max - b.min;
        size = Vector3.ProjectOnPlane(size, Vector3.up);
        return size.magnitude / 2f;
    }

    [ContextMenu("Do it")]
    public void TestNow()
    {
        SetRadius(GetRadius());
    }
}
