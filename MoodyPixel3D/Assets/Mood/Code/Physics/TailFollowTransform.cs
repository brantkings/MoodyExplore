using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class TailFollowTransform : MonoBehaviour
{
    [System.Serializable]
    public struct TailPart
    {
        public Transform part;
        public float minDistanceFromBefore;
        public float maxDistanceFromBefore;
        public bool lookToDirection;
    }

    public TailPart[] tail;
    public Transform target;

    [Space()]
    public FollowMode atStart = FollowMode.None;
    public enum FollowMode
    {
        EnforceTail,
        ForceToMin,
        ForceToMax,
        None
    }
    
    private void Follow(Transform follower, Transform target, float minDistance, float maxDistance, bool lookToDirection)
    {
        Vector3 myPos = follower.position;
        var targetPos = target.position;
        Vector3 distance = targetPos - myPos;
        float newDist = Mathf.Clamp(distance.magnitude, minDistance, maxDistance);
        distance = distance.normalized * newDist;
        myPos = targetPos - distance;
        follower.position = myPos;
        if (lookToDirection) follower.forward = distance;   
    }

    private void Follow(Transform target, TailPart part, FollowMode mode = FollowMode.EnforceTail)
    {
        switch (mode)
        {
            case FollowMode.EnforceTail:
                Follow(part.part, target, part.minDistanceFromBefore, part.maxDistanceFromBefore, part.lookToDirection);
                break;
            case FollowMode.ForceToMin:
                Follow(part.part, target, part.minDistanceFromBefore, part.minDistanceFromBefore, part.lookToDirection);
                break;
            case FollowMode.ForceToMax:
                Follow(part.part, target, part.maxDistanceFromBefore, part.maxDistanceFromBefore, part.lookToDirection);
                break;
            default:
                break;
        }
    }
    private void EnforceTail(Transform tailTarget, FollowMode mode = FollowMode.EnforceTail)
    {
        Transform nowFollow = tailTarget;
        foreach (TailPart part in tail)
        {
            Follow(nowFollow, part, mode);
            nowFollow = part.part;
        }
    }
    

    private void Start()
    {
        EnforceTail(target, atStart);
    }

    private void LateUpdate()
    {
        EnforceTail(target);
    }
}
