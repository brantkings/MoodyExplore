using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform Target;
    public float minDistance;
    public float maxDistance;
    public bool lookToDirection;

    public void LateUpdate()
    {
        Vector3 myPos = transform.position;
        var targetPos = Target.position;
        Vector3 distance = targetPos - myPos;
        float newDist = Mathf.Clamp(distance.magnitude, minDistance, maxDistance);
        distance = distance.normalized * newDist;
        myPos = targetPos - distance;
        transform.position = myPos;
        if (lookToDirection) transform.forward = distance;
    }
}
