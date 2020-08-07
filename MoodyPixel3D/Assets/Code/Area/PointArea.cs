using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointArea : Area
{
    public Vector3 offset;

    public Vector3 GetPoint()
    {
        return transform.TransformPoint(offset);
    }

    public override void DrawGizmo()
    {
        Gizmos.DrawWireSphere(GetPoint(), 0.5f);
    }

    public override Vector3 GetRandomPosition()
    {
        return GetPoint();
    }
}
