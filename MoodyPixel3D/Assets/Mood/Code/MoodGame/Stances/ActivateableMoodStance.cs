using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Mood/Stances/Activateable Mood Stance", fileName = "Stance_A_")]
public class ActivateableMoodStance : MoodStance
{
    [Header("Activateable")]
    [SerializeField]
    public MoodPawnModifier[] pawnModifiers;
    [SerializeField]
    private bool _hasTimeLimit;
    [SerializeField]
    private float _timeLimit;

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

    public void ApplyStance(MoodPawn pawn, bool setWithStance)
    {
        if(pawn.animator != null && !string.IsNullOrEmpty(_stanceAnimParamBool) )
            pawn.animator.SetBool(_stanceAnimParamBool, setWithStance);
        if(_hasTimeLimit && setWithStance)
            pawn.StartCoroutine(TimeoutRoutine(pawn));

        foreach (var mod in pawnModifiers) mod.SetModifierApplied(this, pawn, setWithStance); 
    }

    public virtual IEnumerable<T> GetAllModifiers<T>() where T:class
    {
        if(pawnModifiers != null)
        {
            for (int i = 0, len = pawnModifiers.Length; i < len; i++)
            {
                MoodPawnModifier mod = pawnModifiers[i];
                if (mod is T) yield return mod as T;
            }
        }
    }

    private IEnumerator TimeoutRoutine(MoodPawn pawn)
    {
        //Debug.LogErrorFormat("Starting timeout for {0} {1},", pawn, this);
        yield return new WaitForSeconds(_timeLimit);
        //Debug.LogErrorFormat("Ending timeout for {0} {1},", pawn, this);
        pawn.RemoveStance(this);
    }

    public float GetTimeoutDelay()
    {
        if (_hasTimeLimit) return _timeLimit;
        else return 0f;
    }


}
