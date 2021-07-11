using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChangeParameterFromPawnEffect<T> : MoodPawnEffect<ChangeParameterFromPawnEffect<T>.Status>
{
    public struct Status
    {
        internal int amount;
    }

    protected abstract MoodParameter<T> GetParameterFromPawn(MoodPawn p);

    protected abstract IMoodAlterator<T> GetAlterator(MoodPawn p);

    public enum ListInteractionType
    {
        First,
        Last,
    }

    public ListInteractionType howToAdd = ListInteractionType.Last;
    public ListInteractionType howToRemove = ListInteractionType.Last;

    protected override ChangeParameterFromPawnEffect<T>.Status? GetInitialStatus(MoodPawn p)
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

    protected override void UpdateStatusAdd(MoodPawn p, ref Status? status)
    {
        if(status.HasValue)
        {
            Status v = status.Value;
            v.amount++;
            status = v;

            Debug.LogFormat("{0} added {1} times!", this, v.amount);
            if (v.amount > 20) return;
            switch (howToAdd)
            {
                case ListInteractionType.First:
                    GetParameterFromPawn(p)?.AddAlteratorFirst(GetID(), GetAlterator(p));
                    break;
                case ListInteractionType.Last:
                    Debug.LogFormat("Getting parameter {0} from {1} last with id {2} alterator {3}.", GetParameterFromPawn(p), p, GetID(), GetAlterator(p));
                    GetParameterFromPawn(p)?.AddAlteratorLast(GetID(), GetAlterator(p));
                    break;
                default:
                    break;
            }
        }
    }

    protected override void UpdateStatusRemove(MoodPawn p, ref Status? status)
    {
        if (status.HasValue)
        {
            Status v = status.Value;
            v.amount--;
            if(v.amount > 0)
            {
                status = v;
            }
            else
            {
                status = null;
            }

            switch (howToRemove)
            {
                case ListInteractionType.First:
                    GetParameterFromPawn(p)?.RemoveAlteratorFirst(GetID());
                    break;
                case ListInteractionType.Last:
                    GetParameterFromPawn(p)?.RemoveAlteratorLast(GetID());
                    break;
                default:
                    break;
            }
        }
    }
}

public abstract class ChangeIntFromMoodPawnEffect : ChangeParameterFromPawnEffect<int>
{
    public enum Type
    {
        Sum,
        Multiply,
    }

    public int value;
    public Type type;

    protected override IMoodAlterator<int> GetAlterator(MoodPawn p)
    {
        switch (type)
        {
            case Type.Sum:
                return new SumIntAlterator() { value = value };
            case Type.Multiply:
                return new MultiplyIntAlterator() { value = value };
            default:
                return new SumIntAlterator() { value = value };
        }
    }
}

public abstract class ChangeFloatFromMoodPawnEffect : ChangeParameterFromPawnEffect<float>
{
    public enum Type
    {
        Sum,
        Multiply,
    }

    public float value;
    public Type type;

    protected override IMoodAlterator<float> GetAlterator(MoodPawn p)
    {
        switch (type)
        {
            case Type.Sum:
                return new SumFloatAlterator() { value = value };
            case Type.Multiply:
                return new MultiplyFloatAlterator() { value = value };
            default:
                return new SumFloatAlterator() { value = value };
        }
    }
}
