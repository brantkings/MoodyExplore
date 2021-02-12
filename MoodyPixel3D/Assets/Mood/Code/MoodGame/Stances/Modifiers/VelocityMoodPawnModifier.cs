using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Velocity", fileName = "StMod_Velocity_")]
public class VelocityMoodPawnModifier : ConstantMoodPawnModifier, IMoodPawnModifierVelocity
{
    [SerializeField]
    private ValueModifier movementVelocity;

    public void ModifyVelocity(ref Vector3 velocity)
    {
        if (movementVelocity.IsChange())
        {
            float vel = velocity.magnitude;
            movementVelocity.Modify(ref vel);
            velocity = velocity.normalized * vel;
        }
    }
}
