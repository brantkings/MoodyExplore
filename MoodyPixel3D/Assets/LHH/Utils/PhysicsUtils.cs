using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class PhysicsUtils
{
    public static void Launch(this Rigidbody body, Vector3 to, float duration)
    {
        body.AddForce(GetDirectLaunchVelocity(to - body.position, duration, body.drag), ForceMode.VelocityChange);
    }
    

    /// <summary>
    /// This is supposing that there are no gravity, only the drag if the force is applied. With gravity you'll have to take out the Y.
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="time"></param>
    /// <param name="drag"></param>
    /// <returns></returns>
    public static Vector3 GetDirectLaunchVelocity(Vector3 distance, float time, float drag)
    {
        float KdragUnity = Mathf.Clamp01(1f - drag);

        if (KdragUnity == 0f)
            return distance / time;

        float integralFromVelocityOverTime = 1f - Mathf.Exp(-KdragUnity * time);
        return distance * KdragUnity / integralFromVelocityOverTime;
    }
}
