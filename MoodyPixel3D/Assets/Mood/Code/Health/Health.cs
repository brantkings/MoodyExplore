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
    public const int BASE_SINGLE_UNIT_DAMAGE = 10;

    public int amount;
    public DamageTeam team;
    public GameObject origin;

    public Vector3 attackDirection;
    public Vector3 distanceKnockback;
    public float durationKnockback;
    public float rotationKnockbackAngle;

    public bool unreactable;
    public bool shouldStaggerAnimation;
    public bool ignorePhaseThrough;

    public float stunTime;

    public DamageInfo(int damage = 0, DamageTeam damageTeam = DamageTeam.Neutral, GameObject damageOrigin = null)
    {
        amount = damage;
        team = damageTeam;
        origin = damageOrigin;
        attackDirection = Vector3.zero;
        distanceKnockback = Vector3.zero;
        durationKnockback = 0f;
        rotationKnockbackAngle = 0f;
        unreactable = false;
        ignorePhaseThrough = true;
        shouldStaggerAnimation = true;
        stunTime = 0f;
    }

    public DamageInfo SetOppositeDamageTeam(DamageTeam team)
    {
        switch (team)
        {
            case DamageTeam.Ally:
                team = DamageTeam.Enemy;
                break;
            case DamageTeam.Enemy:
                team = DamageTeam.Ally;
                break;
            default:
                team = DamageTeam.Neutral;
                break;
        }
        return this;
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
    private bool _invulnerable;
    private LHH.Utils.TimeStampSeconds _invulnerableTimer = new LHH.Utils.TimeStampSeconds();

    [SerializeField]
    private bool _phasingThroughAttacks;
    private LHH.Utils.TimeStampSeconds _phaseTimer = new LHH.Utils.TimeStampSeconds();

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

    public bool PhasingThroughAttacks
    {
        get
        {
            return _phasingThroughAttacks || _phaseTimer.IsInTime();
        }
    }

    public bool Invulnerable
    {
        get
        {
            return _invulnerable || _invulnerableTimer.IsInTime();
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
        Debug.LogErrorFormat("Damage {0} with {1}. Can damage? {2}. Life now is {3} (pha:{4} inv:{5})", this, info, CanDamage(info.team), _lifeNow, PhasingThroughAttacks, Invulnerable);
        if (!info.ignorePhaseThrough && PhasingThroughAttacks) return DamageResult.Nothing;
        if (CanDamage(info.team))
        {
            if (Invulnerable) return DamageResult.NotDamagingHit;

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

    public void SetInvulnerable(bool set)
    {
        _invulnerable = set;
    }

    public void SetInvulnerableTimer(float time)
    {
        _invulnerableTimer.StartTimer(time);
    }

    public void SetPhaseThroughAttacks(bool set)
    {
        _phasingThroughAttacks = set;
    }

    public void SetPhaseTimer(float time)
    {
        _phaseTimer.StartTimer(time);
    }

    public void Die(DamageInfo dmg)
    {
        OnDeath?.Invoke(dmg, this);
        if (toDestroy == null) toDestroy = gameObject;
        toDestroy.SetActive(false);
        Destroy(toDestroy);
    }
}
