using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Pawn/Condition/Velocity", fileName = "Cond_Vel_")]
public class MoodPawnVelocityCondition : MoodPawnCondition
{
    public float minVel = 0f;
    public float maxVel = float.PositiveInfinity;
    public float maxAngleWithDirection = 180f;

    public override bool ConditionIsOK(MoodPawn pawn)
    {
        Vector3 velocity = pawn.Velocity;
        Vector3 direction = pawn.Direction;
        return LengthOK(velocity.magnitude) && AngleOK(velocity, direction);
    }

    private bool LengthOK(float velocityLength)
    {
        return Mathf.Clamp(velocityLength, minVel, maxVel) == velocityLength;
    }

    private bool AngleOK(Vector3 velocity, Vector3 direction)
    {
        if (maxAngleWithDirection >= 180f) return true;
        else return Vector3.Angle(velocity, direction) < maxAngleWithDirection;
    }
}
