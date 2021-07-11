using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChangeMoodPawnEffect : MoodPawnEffect<ChangeMoodPawnEffect.Status>
{
    public struct Status
    {
        internal int amount;
    }

    public enum ListInteractionType
    {
        First,
        Last,
    }

    public ListInteractionType howToAdd = ListInteractionType.Last;
    public ListInteractionType howToRemove = ListInteractionType.Last;

    protected override ChangeMoodPawnEffect.Status? GetInitialStatus(MoodPawn p)
    {
        return new Status
        {
            amount = 0,
        };
    }

    protected virtual string GetID()
    {
        return name;
    }

    protected abstract void AddChange(MoodPawn p);
    protected abstract void RemoveChange(MoodPawn p);

    protected override void UpdateStatusAdd(MoodPawn p, ref Status? status)
    {
        if (status.HasValue)
        {
            Status v = status.Value;
            v.amount++;
            status = v;

            AddChange(p);
        }
    }

    protected override void UpdateStatusRemove(MoodPawn p, ref Status? status)
    {
        if (status.HasValue)
        {
            Status v = status.Value;
            v.amount--;
            if (v.amount > 0)
            {
                status = v;
            }
            else
            {
                status = null;
            }

            RemoveChange(p);
        }
    }
}
