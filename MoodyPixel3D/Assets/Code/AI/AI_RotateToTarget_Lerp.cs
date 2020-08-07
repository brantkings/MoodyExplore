using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_RotateToTarget_Lerp : AI_RotateToTarget
{
    [Space()]
    public float smoothTime;
    public float maxVectorDistance = float.PositiveInfinity;

    private Vector3 _currentDir;
    private Vector3 _dirDelta;

    protected override void SetDirection(Transform who, Vector3 dir)
    {
        _currentDir = Vector3.SmoothDamp(_currentDir, dir.normalized, ref _dirDelta, smoothTime, maxVectorDistance);
        base.SetDirection(who, _currentDir);
    }

}
