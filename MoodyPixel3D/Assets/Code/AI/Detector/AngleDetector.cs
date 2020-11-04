using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Utils;

public class AngleDetector : PreTargetDetector
{

    [Header("Angle")]
    public DirectionManipulator angleGetter;
    public float maxAngleThatCountsAsIn = 90f;
    public bool useMaxDistance;
    public float maxDistance = 50f;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosUtils.GetSuccessColor(IsInAngle());
        Vector3 origin = angleGetter.GetTransformOrigin(transform).position, dir = angleGetter.Get(transform);
        GizmosUtils.DrawCone(origin, dir, maxAngleThatCountsAsIn, 2f, 4);
        if(useMaxDistance)
        {
            GizmosUtils.DrawCone(origin + dir * maxDistance, dir, maxAngleThatCountsAsIn, 2f, 4);
        }
    }

    void Update()
    {
        TryUpdateDetecting(IsInAngle());
    }

    private bool IsInAngle()
    {
        Vector3? dist = GetDistanceToTarget();
        if (dist.HasValue)
        {
            if (useMaxDistance)
                if (dist.Value.magnitude > maxDistance)
                    return false;

            if (Vector3.Angle(angleGetter.Get(transform), dist.Value) <= maxAngleThatCountsAsIn)
                return true;
            else
                return false;
        }
        return false;
    }
}
