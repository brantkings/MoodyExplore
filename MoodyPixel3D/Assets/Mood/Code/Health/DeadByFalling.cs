using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeadByFalling : AddonBehaviour<Health>
{
    public DeadByFallingData overwritteableData;

    public Transform customHeightOrigin;

    private Transform PositionGetter
    {
        get
        {
            if (customHeightOrigin != null) return customHeightOrigin;
            else return transform;
        }
    }

    public void Update()
    {
        if (overwritteableData.IsDead(PositionGetter.position)) Addon.Kill();
    }

}
