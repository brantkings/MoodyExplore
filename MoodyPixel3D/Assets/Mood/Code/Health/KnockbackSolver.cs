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
    private float knockbackFromForce = 0f;
    [SerializeField]
    private float knockbackPositionDifferenceNormalized = 0f;
    [SerializeField]
    private Vector3 knockbackPositionDifferenceProjectedOnPlane = Vector3.zero;

    [SerializeField]
    private bool totalMagnitudeConstant;
    [SerializeField]
    private float constantTotalMagnitude = 1;

    [Space()]
    [SerializeField]
    private float angleRotation;

    [Space()]
    [SerializeField]
    private float knockbackDuration = 0.25f;

    public float GetDuration()
    {
        return knockbackDuration;
    }

    public Vector3 GetKnockback(Transform from, Transform to, out float knockbackAngle)
    {
        return GetKnockback(from, to, Vector3.zero, out knockbackAngle);
    }

    public Vector3 GetKnockback(Transform from, Transform to, Vector3 attackForce, out float knockbackAngle)
    {
        Vector3 knock = knockbackByDealer.Get(from) + knockbackByReceiver.Get(from) + absoluteKnockback + GetKnockbackPositionDifference(from, to) + knockbackFromForce * attackForce;
        Debug.LogFormat("Knockback: [From:{0} To:{1} Abs:{2}; PosDif:{3} (to:{7} - from:{8}='{9}'); Force:{4}] --> Total:{5} ({6})",
            knockbackByDealer.Get(from), knockbackByReceiver.Get(from), absoluteKnockback, GetKnockbackPositionDifference(from, to), knockbackFromForce * attackForce,
            knock.normalized * constantTotalMagnitude, knock,
            to.position,from.position, to.position - from.position);
        if (totalMagnitudeConstant) knock = knock.normalized * constantTotalMagnitude;

        if (angleRotation != 0f)
        {
            knockbackAngle = Mathf.Sign(Vector3.SignedAngle(attackForce, to.forward, Vector3.up)) * angleRotation;
        }
        else knockbackAngle = 0f;

        return knock;
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
