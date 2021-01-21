using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_ConstantForceToTarget : AI_ForceRigibody
{
    [Header("Target")]
    public Detector toGetTarget;
    public bool distanceDependent;

    private void FixedUpdate()
    {
        Vector3? distance = toGetTarget?.GetDistanceToTarget(); 
        if(distance.HasValue)
            Force(GetForceValue(distance.Value));
    }

    private Vector3 GetForceValue(Vector3 distance)
    {
        if (distanceDependent) return distance;
        else return distance.normalized;
    }
}
