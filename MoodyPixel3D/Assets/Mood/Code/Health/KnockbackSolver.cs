using UnityEngine;
using System;


[Serializable]
public class KnockbackSolver
{
    [SerializeField]
    private RelativeVector3 knockbackByDealer = Direction.Forward;
    [SerializeField]
    private RelativeVector3 knockbackByReceiver = Direction.Back;
    [SerializeField]
    private Vector3 absoluteKnockback = Vector3.zero;
    [SerializeField]
    private float knockbackFromHitbox = 0f;
    [SerializeField]
    private float knockbackPositionDifferenceNormalized = 0f;
    [SerializeField]
    private Vector3 knockbackPositionDifferenceProjectedOnPlane = Vector3.zero;

    [Space()]
    [SerializeField]
    private float angleRotation;
    [Space]
    [SerializeField] private MoodUnitManager.TimeBeats knockbackDurationAbsolute = 0;
    [SerializeField] private MoodUnitManager.TimeBeats knockbackDurationMultiplierForDistance = 1;

    public float GetDuration(MoodUnitManager.DistanceBeats knockbackDistance)
    {
        return MoodUnitManager.GetTime(knockbackDurationAbsolute.beats + knockbackDistance.beats * knockbackDurationMultiplierForDistance.beats);  
    }

    public Vector3 GetKnockback(Transform from, Transform to, float magnitude, out float knockbackAngle)
    {
        return GetKnockback(from, to, Vector3.zero, magnitude, out knockbackAngle);
    }

    public Vector3 GetKnockback(Transform from, Transform to, Vector3 attackForce, float magnitude, out float knockbackAngle)
    {
        Vector3 knock = knockbackByDealer.Get(from) + knockbackByReceiver.Get(from) + absoluteKnockback + GetKnockbackPositionDifference(from, to) + knockbackFromHitbox * attackForce;
        Debug.LogFormat("Knockback: [From:{0} To:{1} Abs:{2}; PosDif:{3} (to:{7} - from:{8}='{9}'); Force:{4}] --> Total:{5} ({6})",
            knockbackByDealer.Get(from), knockbackByReceiver.Get(from), absoluteKnockback, GetKnockbackPositionDifference(from, to), knockbackFromHitbox * attackForce,
            knock.normalized, knock,
            to.position,from.position, to.position - from.position);

        if (angleRotation != 0f)
        {
            knockbackAngle = Mathf.Sign(Vector3.SignedAngle(attackForce, to.forward, Vector3.up)) * angleRotation;
        }
        else knockbackAngle = 0f;

        return knock.normalized * magnitude;
    }

    private Vector3 GetKnockbackPositionDifference(Transform from, Transform to)
    {
        Vector3 dif = (to.position - from.position).normalized * knockbackPositionDifferenceNormalized;
        if (dif != Vector3.zero && knockbackPositionDifferenceProjectedOnPlane != Vector3.zero)
        {
            dif = Vector3.ProjectOnPlane(dif, knockbackPositionDifferenceProjectedOnPlane);
        }
        return dif;
    }
}
