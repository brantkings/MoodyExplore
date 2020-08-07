using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TransformGetter
{
    public Transform custom;

    public Transform Get(Transform origin)
    {
        if (custom != null) return custom;
        else return origin;
    }
}
