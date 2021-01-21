using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SamePosition : MonoBehaviour
{
    public enum ScaleMode
    {
        Nothing,
        LossyToLocal,
        LocalToLocal
    }

    public Transform target;

    public bool samePosition = true;
    public bool sameRotation = true;
    public ScaleMode sameScale = ScaleMode.Nothing;
    
    public void SetLikeTarget()
    {
        if (samePosition) transform.position = target.position;
        if (sameRotation) transform.rotation = target.rotation;
        switch (sameScale)
        {
            case ScaleMode.Nothing:
                break;
            case ScaleMode.LossyToLocal:
                transform.localScale = target.lossyScale;
                break;
            case ScaleMode.LocalToLocal:
                transform.localScale = target.localScale;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
}
