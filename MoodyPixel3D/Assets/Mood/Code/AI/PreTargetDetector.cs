using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PreTargetDetector : Detector
{
    [Header("Target")]
    public Transform Target;
    public bool getPlayer;

    protected virtual void OnValidate()
    {
        if (getPlayer) UpdateTarget(GetPlayerTransform());
    }

    protected virtual void Start()
    {
        if (getPlayer) UpdateTarget(GetPlayerTransform());
    }

    public Transform GetPlayerTransform()
    {
        return MoodPlayerController.Instance?.transform;
    }

}
