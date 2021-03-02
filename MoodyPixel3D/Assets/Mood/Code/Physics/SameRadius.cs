using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SameRadius<T> : LHH.Unity.AddonParentBehaviour<T> where T:Component
{
    public float radiusToScaleRatio = 1f;
    public float defaultRadius = 1f;

    // Start is called before the first frame update
    void Start()
    {
        SetRadius(GetRadius());
    }

    public void SetRadius(float radius)
    {
        transform.localScale = Vector3.one * radius * radiusToScaleRatio;
    }

    public abstract float GetRadius();
}
