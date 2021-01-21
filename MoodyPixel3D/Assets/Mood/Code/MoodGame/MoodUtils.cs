using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ValueModifier
{
    public float add = 0f;
    public float multiplier = 1f;

    public void Modify(ref float value)
    {
        value = value * multiplier + add;
    }

    public void Modify(ref int value, System.Func<float, int> toInt)
    {
        value = toInt((float)value * multiplier + add);
    }

    public bool IsChange()
    {
        return add != 0f || multiplier != 1f;
    }
}
