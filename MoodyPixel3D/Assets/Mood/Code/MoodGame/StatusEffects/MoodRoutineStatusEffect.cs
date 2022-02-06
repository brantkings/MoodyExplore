using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodRoutineStatusEffect : MoodStatusEffectState<Coroutine>
{
    public override Coroutine AddEffectWithState(MoodPawn pawn)
    {
        return pawn.StartCoroutine(StatusEffectRoutine(pawn));
    }

    public override void RemoveEffectWithState(MoodPawn pawn, Coroutine state)
    {
        pawn.StopCoroutine(state);
    }

    protected abstract IEnumerator StatusEffectRoutine(MoodPawn pawn);

}
