using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodStatusEffectState<T> : MoodStatusEffect where T:class
{
    public static Dictionary<MoodPawn, T> _states = new Dictionary<MoodPawn, T>(4);

    protected T GetState(MoodPawn p)
    {
        if (_states.ContainsKey(p))
            return _states[p];
        else return null;
    }

    public override void AddEffect(MoodPawn pawn)
    {
        _states.Add(pawn, AddEffectWithState(pawn));
    }

    public override void RemoveEffect(MoodPawn pawn)
    {
        RemoveEffectWithState(pawn, GetState(pawn));
        _states.Remove(pawn);
    }

    public abstract T AddEffectWithState(MoodPawn pawn);

    public abstract void RemoveEffectWithState(MoodPawn pawn, T state);

}
