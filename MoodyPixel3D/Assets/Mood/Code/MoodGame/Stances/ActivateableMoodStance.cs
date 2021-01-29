using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private bool _stun;

    [SerializeField]
    private string _stanceAnimParamBool;

    [Header("Flags")]
    [SerializeField]
    private MoodEffectFlag[] flags;
    [SerializeField]
    private MoodEffectFlag[] deactivateFlags;

    public bool HasFlag(MoodEffectFlag flag)
    {
        return HasFlagToActivate(flag) || HasFlagToDeactivate(flag);
    }

    public bool HasFlagToActivate(MoodEffectFlag flag)
    {
        return flags.Contains(flag);
    }

    public bool HasFlagToDeactivate(MoodEffectFlag flag)
    {
        return deactivateFlags.Contains(flag);
    }

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

    public void ApplyStance(MoodPawn pawn, bool setWithStance)
    {
        if(pawn.animator != null && !string.IsNullOrEmpty(_stanceAnimParamBool) )
            pawn.animator.SetBool(_stanceAnimParamBool, setWithStance);
        if(_hasTimeLimit && setWithStance)
            pawn.StartCoroutine(TimeoutRoutine(pawn));
        if (_stun)
        {
            if (setWithStance)
                pawn.AddStunLock(name);
            else
                pawn.RemoveStunLock(name);
        }
    }

    private IEnumerator TimeoutRoutine(MoodPawn pawn)
    {
        yield return new WaitForSeconds(_timeLimit);
        pawn.RemoveStance(this);
    }


}
