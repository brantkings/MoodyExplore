using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IMoodAlterator<T>
{
    T Alterate(T value);
}

public abstract class SingleValueAlterator<T> : IMoodAlterator<T>
{
    public T value;
    public abstract T Alterate(T entry);
}

public class SumIntAlterator : SingleValueAlterator<int>
{
    public override int Alterate(int entry)
    {
        return value + entry;
    }
}

public class MultiplyIntAlterator : SingleValueAlterator<int>
{
    public override int Alterate(int entry)
    {
        return value * entry;
    }
}

public class SumFloatAlterator : SingleValueAlterator<float>
{
    public override float Alterate(float entry)
    {
        return value + entry;
    }
}

public class MultiplyFloatAlterator : SingleValueAlterator<float>
{
    public override float Alterate(float entry)
    {
        return value * entry;
    }
}

[System.Serializable]
public class MoodParameter<T>
{
    [SerializeField]
    private T baseValue;
    [SerializeField]
    [HideInInspector]
    private LinkedList<SingleAlt> alterations = new LinkedList<SingleAlt>();

    public delegate void DelChange(T before, T after);
    public event DelChange OnChange;

    [System.Serializable]
    private struct SingleAlt
    {
        internal string id;
        internal IMoodAlterator<T> alteration;
    }

    private SingleAlt Create(string id, IMoodAlterator<T> alterator)
    {
        return new SingleAlt()
        {
            id = id,
            alteration = alterator
        };

    }

    private int i = 0;
    public void AddAlteratorLast(string id, IMoodAlterator<T> alterator)
    {
        T before = GetAlteratedValue();
        alterations.AddLast(Create(id, alterator));
        Debug.LogFormat("{0} is adding {1}", this, i++);
        if (i > 20) return;
        //OnChange?.Invoke(before, GetAlteratedValue());
    }

    public void AddAlteratorFirst(string id, IMoodAlterator<T> alterator)
    {
        T before = GetAlteratedValue();
        alterations.AddFirst(Create(id, alterator));
        Debug.LogFormat("{0} is adding {1}", this, i++);
        if (i > 20) return;
        //OnChange?.Invoke(before, GetAlteratedValue());
    }

    public void RemoveAlteratorFirst(string id)
    {
        System.Func<SingleAlt, bool> findID = (x) => x.id == id;
        if(alterations.Any(findID))
        {
            T before = GetAlteratedValue();
            alterations.Remove(alterations.First(findID));
            OnChange?.Invoke(before, GetAlteratedValue());
        }
    }

    public void RemoveAlteratorLast(string id)
    {
        System.Func<SingleAlt, bool> findID = (x) => x.id == id;
        if (alterations.Any(findID))
        {
            T before = GetAlteratedValue();
            alterations.Remove(alterations.Last(findID));
            OnChange?.Invoke(before, GetAlteratedValue());
        }
    }

    public T GetBaseValue()
    {
        return baseValue;
    }

    public void SetBaseValue(T value)
    {
        T oldVal = baseValue;
        baseValue = value;
        if(!oldVal.Equals(baseValue))
        {
            OnChange?.Invoke(GetAlteratedValue(oldVal), GetAlteratedValue(baseValue));
        }
    }

    public T GetAlteratedValue()
    {
        return GetAlteratedValue(baseValue);
    }

    public T GetAlteratedValue(T fromValue)
    {
        int j = 0;
        //Debug.LogFormat("Getting alterated value from '{0}' with {1} alterations", fromValue, alterations.Count);
        foreach (SingleAlt alt in alterations)
        {
            // Debug.LogFormat("{0} has alt {1} ({4}) {2}/{3}", this, alt.id, j++, alterations.Count, alt.alteration);
            fromValue = alt.alteration.Alterate(fromValue);
        }
        return fromValue;
    }

    public static implicit operator MoodParameter<T>(T v)
    {
        return new MoodParameter<T>()
        {
            baseValue = v
        };
    }

    public static implicit operator T(MoodParameter<T> v)
    {
        if (v == null) return default(T);
        else return v.GetAlteratedValue();
    }
}

