using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameRadiusAsParentPawn : LHH.Unity.AddonParentBehaviour<MoodPawn>
{
    public float radiusToScaleRatio = 1f;
    public float defaultRadius = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Collider col = Addon.Health.GetComponentInChildren<Collider>();
        if(col != null)
        {
            Vector3 size = col.bounds.max - col.bounds.min;
            size = Vector3.ProjectOnPlane(size, Vector3.up);
            SetRadius(size.magnitude/2f);
        }
        else
        {
            SetRadius(defaultRadius);
        }
    }

    public void SetRadius(float radius)
    {
        transform.localScale = Vector3.one * radius * radiusToScaleRatio;
    }

}
