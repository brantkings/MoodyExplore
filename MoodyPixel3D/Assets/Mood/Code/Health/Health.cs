using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum DamageTeam
{
    Ally,
    Enemy,
    Neutral
}

public struct DamageInfo
{
    public int amount;
    public DamageTeam team;
    public GameObject origin;

    public Vector3 attackDirection;
    public Vector3 distanceKnockback;
    public float durationKnockback;
    public float rotationKnockbackAngle;

    public bool shouldStaggerAnimation;

    public float stunTime;

    public DamageInfo(int damage = 0, DamageTeam damageTeam = DamageTeam.Neutral, GameObject damageOrigin = null)
    {
        amount = damage;
        team = damageTeam;
        stunTime = 0f;
        origin = damageOrigin;
        attackDirection = Vector3.zero;
        distanceKnockback = Vector3.zero;
        durationKnockback = 0f;
        rotationKnockbackAngle = 0f;
        shouldStaggerAnimation = true;
    }

    public DamageInfo SetDirection(Vector3 direction)
    {
        attackDirection = direction;
        return this;
    }

    public DamageInfo SetForce(Vector3 knockback, float angleRotation, float duration)
    {
        distanceKnockback = knockback;
        rotationKnockbackAngle = angleRotation;
        durationKnockback = duration;
        return this;
    }

    public DamageInfo SetStaggerAnimation(bool animate)
    {
        shouldStaggerAnimation = animate;
        return this;
    }

    public DamageInfo SetStunTime(float time)
    {
        stunTime = time;
        return this;
    }

    public override string ToString()
    {
        return string.Format("[{0} damage with '{1}' by '{2}']", amount, team, origin);
    }
}

public class Health : MonoBehaviour {

    public enum DamageResult
    {
        Nothing = 0,
        DamagingHit,
        NotDamagingHit,
        KillingHit,
        HealHit,
    }

    [SerializeField]
    private int _maxLife = 10;

    [SerializeField]
    [ReadOnly]
    private int _lifeNow;

    public DamageTeam damageFrom;

    public GameObject toDestroy;

    public delegate void DelHealthFeedback(DamageInfo damage, Health damaged);
    public delegate void DelHealthModifier(ref DamageInfo damage, Health damaged);
    public event DelHealthModifier OnBeforeDamage;
    public event DelHealthFeedback OnDamage;
    public event DelHealthFeedback OnDeath;

    public int MaxLife
    {
        get
        {
            return _maxLife;
        }
    }

    public int Life
    {
        get
        {
            return _lifeNow;
        }
    }

    public float Ratio
    {
        get
        {
            return (float)_lifeNow / _maxLife;
        }
    }

    private void Start()
    {
        _lifeNow = _maxLife;
    }

    public bool IsAlive()
    {
        return _lifeNow > 0;
    }

    public virtual DamageResult Damage(DamageInfo info)
    {
        //Debug.LogErrorFormat("Damage {0} with {1},{2}. Can damage? {3}. Life now is {4}", this, info.amount, info.team, CanDamage(info.team), _lifeNow);
        if (CanDamage(info.team))
        {
            OnBeforeDamage?.Invoke(ref info, this);
            int lifeBefore = _lifeNow;
            _lifeNow = Mathf.Clamp(_lifeNow - info.amount, 0, _maxLife);
            //Debug.LogErrorFormat("{0} is dead? {1} <= 0 so {2}", this, _lifeNow, _lifeNow <= 0);
            OnDamage?.Invoke(info, this);
            if (_lifeNow <= 0)
            {
                Die(info);
                return DamageResult.KillingHit;
            }

            if (_lifeNow > lifeBefore) return DamageResult.HealHit;
            else if (_lifeNow < lifeBefore) return DamageResult.DamagingHit;
            else return DamageResult.NotDamagingHit;

        }
        return DamageResult.Nothing;
    }

    public static bool IsDamage(DamageResult result)
    {
        return result != DamageResult.Nothing && result != DamageResult.HealHit;
    }

    public void Kill(DamageTeam team = DamageTeam.Neutral, GameObject origin = null)
    {
        Damage(new DamageInfo(_maxLife, team, origin));
    }

    protected bool CanDamage(DamageTeam from)
    {
        switch (from)
        {
            case DamageTeam.Neutral:
                return true;
            default:
                return from == damageFrom;
        }
    }

    public void Die(DamageInfo dmg)
    {
        OnDeath?.Invoke(dmg, this);
        if (toDestroy == null) toDestroy = gameObject;
        toDestroy.SetActive(false);
        Destroy(toDestroy);
    }
}
