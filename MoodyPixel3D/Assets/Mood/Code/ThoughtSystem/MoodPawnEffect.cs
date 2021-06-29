using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class MoodPawnEffect : ScriptableObject
{
    public abstract void AddEffect(MoodPawn pawn);
    public abstract void RemoveEffect(MoodPawn pawn);
}

public abstract class MoodPawnEffect<T> : MoodPawnEffect where T:struct
{
    private static Dictionary<ValueTuple<MoodPawn, MoodPawnEffect<T>>, T> _dict = new Dictionary<ValueTuple<MoodPawn, MoodPawnEffect<T>>, T>();

    private ValueTuple<MoodPawn, MoodPawnEffect<T>> GetKey(MoodPawn p)
    {
        return (p, this);
    }

    protected bool InEffect(MoodPawn p)
    {
        return _dict.ContainsKey(GetKey(p));
    }

    protected T? GetStatus(MoodPawn p) 
    {
        if (InEffect(p)) return _dict[GetKey(p)];
        else return null;
    }

    protected void SetStatus(MoodPawn p, T status)
    {
        var key = GetKey(p);
        if(_dict.ContainsKey(key))
        {
            _dict[key] = status;
        }
        else
        {
            _dict.Add(key, status);
        }
    }

    protected void RemoveStatus(MoodPawn p)
    {
        _dict.Remove(GetKey(p));
    }

    private void SetStatus(MoodPawn p, T? statusNullable)
    {
        if(statusNullable.HasValue)
        {
            SetStatus(p, statusNullable.Value);
        }
        else
        {
            RemoveStatus(p);
        }
    }

    protected abstract T? GetInitialStatus(MoodPawn p);

    /// <summary>
    /// Updates when the effect wants to add more. Return status as null to remove it.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="status"></param>
    protected abstract void UpdateStatusAdd(MoodPawn p, ref T? status);


    /// <summary>
    /// Updates when the effect wants to remove more. Turn status as null to remove it.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="status"></param>
    protected abstract void UpdateStatusRemove(MoodPawn p, ref T? status);

    public override void AddEffect(MoodPawn pawn)
    {
        T? stat;
        if (!InEffect(pawn)) stat = GetInitialStatus(pawn);
        else stat = GetStatus(pawn);
        UpdateStatusAdd(pawn, ref stat);
        SetStatus(pawn, stat);
    }

    public override void RemoveEffect(MoodPawn pawn)
    {
        T? stat = GetStatus(pawn);
        UpdateStatusRemove(pawn, ref stat);
        SetStatus(pawn, stat);
    }


}
