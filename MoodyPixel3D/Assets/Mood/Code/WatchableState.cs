using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchableState<T>
{
    private T state;
    public delegate void DelOnChanged(T change);
    public event DelOnChanged OnChanged;

    public static implicit operator T(WatchableState<T> b)
    {
        return b.state;
    }

    public static explicit operator WatchableState<T>(T b)
    {
        return new WatchableState<T>(){state = b};
    }

    internal void Update(T newState)
    {
        if (!state.Equals(newState))
        {
            this.state = newState;
            if (OnChanged != null) OnChanged(newState);
        }
    }

    public void Update(T newState, string debug)
    {
        if (!state.Equals(newState))
        {
            //Debug.LogFormat("Changed {0} to {1}", debug, newState);
            this.state = newState;
            if (OnChanged != null) OnChanged(newState);
        }
    }

    public override string ToString()
    {
        return state.ToString();
    }
}
