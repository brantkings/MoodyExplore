using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ReactionInfo
{
    public GameObject origin;
    public Vector3 direction;
    public Vector3 counterNormal;
    public float duration;
    public int intensity;

    public static explicit operator ReactionInfo(DamageInfo info)
    {
        return new ReactionInfo()
        {
            origin = info.origin,
            intensity = info.amount,
            direction = info.distanceKnockback,
            duration = info.durationKnockback,
            counterNormal = info.attackDirection
        };
    }

    public ReactionInfo SetNormal(Vector3 normal)
    {
        this.counterNormal = normal;
        return this;
    }

    public ReactionInfo(GameObject origin, Vector3 knockback, float duration)
    {
        this.origin = origin;
        this.intensity = 0;
        this.duration = duration;
        this.direction = knockback;

        this.counterNormal = Vector3.zero;
    }

    public int GetDamage()
    {
        return intensity;
    }

    public override string ToString()
    {
        return string.Format("[{0} by {1}, {2} intensity, {3} duration]", direction, origin?.name, intensity, duration);
    }

}
