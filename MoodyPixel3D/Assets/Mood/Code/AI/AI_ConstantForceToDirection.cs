using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Utils;

public class AI_ConstantForceToDirection : AI_ForceRigibody
{
    [Header("Direction")]
    public RelativeVector3 direction = Direction.Forward;
    public bool useAbsoluteDirection;
    public Vector3 absoluteDirection = Vector3.forward;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosUtils.AI_Color;
        GizmosUtils.DrawArrow(transform.position, transform.position + GetDirection(), 0.5f);
    }
    
    private void FixedUpdate()
    {
        Force(GetDirection());
    }

    private Vector3 GetDirection()
    {
        if (useAbsoluteDirection) return absoluteDirection;
        else return direction.Get(transform);
    }
}
