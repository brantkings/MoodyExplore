using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodPawnUpdateEffect
{
    void UpdatePawn(MoodPawn pawn);
}

public interface IMoodPawnStatusEffect
{
    string GetName();

    void AddEffect(MoodPawn pawn);
    void RemoveEffect(MoodPawn pawn);
}

public abstract class MoodStatusEffect : ScriptableObject, IMoodPawnStatusEffect
{
    [SerializeField] private string _statusEffectName;
    [SerializeField] private string _statusEffectDiscription;

    public string GetName()
    {
        return _statusEffectName;
    }

    public abstract void AddEffect(MoodPawn pawn);
    public abstract void RemoveEffect(MoodPawn pawn);
}
