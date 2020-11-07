using UnityEngine;
using System;


[Serializable]
public class KnockbackSolver
{
    [SerializeField]
    private RelativeVector3 knockbackByTransform = Direction.Forward;
    [SerializeField]
    private Vector3 knockbackConstant = Vector3.zero;
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
    private float knockbackDuration = 0.25f;

    public float GetDuration()
    {
        return knockbackDuration;
    }

    public Vector3 GetKnockback(Transform from, Transform to)
    {
        Vector3 knock = knockbackByTransform.Get(from) + knockbackConstant + GetKnockbackPositionDifference(from, to);
        if (totalMagnitudeConstant) knock = knock.normalized * constantTotalMagnitude;
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
