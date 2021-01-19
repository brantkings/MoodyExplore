using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Stances/Activateable Mood Stance", fileName = "Stance_A_")]
public class ActivateableMoodStance : MoodStance
{
    [Header("Activateable")]
    [SerializeField]
    private ValueModifier staminaOverTimeIdle;
    [SerializeField]
    private ValueModifier staminaOverTimeMoving;
    [SerializeField]
    private ValueModifier movementVelocity;
    


    [SerializeField]
    private bool _hasTimeLimit;
    [SerializeField]
    private float _timeLimit;


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
        if(_hasTimeLimit && withStance)
            pawn.StartCoroutine(TimeoutRoutine(pawn));
    }

    private IEnumerator TimeoutRoutine(MoodPawn pawn)
    {
        yield return new WaitForSeconds(_timeLimit);
        pawn.RemoveStance(this);
    }


}
