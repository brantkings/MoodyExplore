using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineRendererAsChildren : AddonBehaviour<LineRenderer>
{
    private void LateUpdate()
    {
        Addon.positionCount = transform.childCount;
        int i = 0;
        foreach(Transform t in transform)
        {
            Addon.SetPosition(i++, t.position);
        }
    }
}
