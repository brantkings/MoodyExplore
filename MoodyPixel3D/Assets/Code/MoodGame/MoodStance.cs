using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Mood Stance", fileName = "Stance_")]
public class MoodStance : ScriptableObject
{
    [System.Serializable]
    public class ValueModifier
    {
        public float add = 0f;
        public float multiplier = 1f;

        public void Modify(ref float value)
        {
            value = value * multiplier + add;
        }

        public bool IsChange()
        {
            return add != 0f || multiplier != 1f;
        }
    }

    [SerializeField]
    private ValueModifier staminaOverTimeIdle;
    [SerializeField]
    private ValueModifier staminaOverTimeMoving;
    [SerializeField]
    private ValueModifier movementVelocity;


    [SerializeField]
    private string _stanceAnimParamBool;

    public void ModifyStamina(ref float stamina, bool moving)
    {
        if (moving) staminaOverTimeMoving.Modify(ref stamina);
        else staminaOverTimeIdle.Modify(ref stamina);
    }

    public void ModifyVelocity(ref Vector3 velocity)
    {
        if(movementVelocity.IsChange())
        {
            float vel = velocity.magnitude;
            movementVelocity.Modify(ref vel);
            velocity = velocity.normalized * vel;
        }
    }

    public void ApplyStance(MoodPawn pawn, bool withStance)
    {
        if(pawn.animator != null && !string.IsNullOrEmpty(_stanceAnimParamBool) )
            pawn.animator.SetBool(_stanceAnimParamBool, withStance);
    }

}
