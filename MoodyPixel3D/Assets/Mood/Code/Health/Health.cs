using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using DG.Tweening;

public enum DamageTeam
{
    Ally,
    Enemy,
    Neutral
}

public struct DamageInfo
{
    public const int BASE_SINGLE_UNIT_DAMAGE = 10;

    public int damage;
    public DamageTeam team;
    public GameObject origin;

    public Vector3 attackDirection;
    public Vector3 distanceKnockback;
    public float durationKnockback;
    public float rotationKnockbackAngle;

    public bool unreactable;
    public bool shouldStaggerAnimation;
    public bool ignorePhaseThrough;

    public struct ThoughtInDamage
    {
        public FlyingThoughtInstance flyingThoughtInstance;
    }

    public List<ThoughtInDamage> pain;

    public bool feedbacks;
    public struct FreezeFrameData
    {
        public int freezeFrameDelay;
        public float freezeFrameDelayRealTime;
        public int freezeFrameAdd;
        public float freezeFrameMult;
        public float freezeFrameTweenDuration;
        public Ease freezeFrameEase;
    }
    public FreezeFrameData freezeFrame;

    public float stunTime;

    public DamageInfo(int damage = 0, DamageTeam damageTeam = DamageTeam.Neutral, GameObject damageOrigin = null)
    {
        this.damage = damage;
        team = damageTeam;
        origin = damageOrigin;
        attackDirection = Vector3.zero;
        distanceKnockback = Vector3.zero;
        durationKnockback = 0f;
        rotationKnockbackAngle = 0f;
        unreactable = false;
        ignorePhaseThrough = false;
        shouldStaggerAnimation = true;
        feedbacks = true;
        pain = null;
        freezeFrame = new FreezeFrameData()
        {
            freezeFrameDelay = 1,
            freezeFrameDelayRealTime = 0.04f,
            freezeFrameAdd = 5,
            freezeFrameMult = 0.65f,
            freezeFrameTweenDuration = 0.03f,
            freezeFrameEase = Ease.OutCirc
        };
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

    public DamageInfo SetDirection(in Vector3 direction)
    {
        attackDirection = direction;
        return this;
    }

    public DamageInfo SetForce(in Vector3 knockback, float angleRotation, float duration)
    {
        distanceKnockback = knockback;
        rotationKnockbackAngle = angleRotation;
        durationKnockback = duration;
        return this;
    }

    public DamageInfo SetIgnorePhaseThrough(bool ignorePhaseThrough)
    {
        this.ignorePhaseThrough = ignorePhaseThrough;
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

    public DamageInfo SetFeedback(bool set)
    {
        feedbacks = set;
        return this;
    }

    public DamageInfo AddPainThought(in FlyingThoughtInstance f) 
    {
        if (pain == null) pain = new List<ThoughtInDamage>(2);
        pain.Add(new ThoughtInDamage() {flyingThoughtInstance = f});
        return this;
    }

    public DamageInfo AddFreezeFrameFeedback(in FreezeFrameData data)
    {
        this.freezeFrame = data;
        return this;
    }

    public TimeManager.FrameFreezeData GetFrameFreeze()
    {
        return new TimeManager.FrameFreezeData()
        {
            delayDuration = freezeFrame.freezeFrameDelayRealTime,
            delayFrames = freezeFrame.freezeFrameDelay,
            freezeDuration = Mathf.FloorToInt(damage * freezeFrame.freezeFrameMult + freezeFrame.freezeFrameAdd) / 60f, //If you put in frames, freeze frames will have different feeling durations based on timedelta
            freezeFrames = 0,
            minTimeScale = 0f,
            tweenDuration = freezeFrame.freezeFrameTweenDuration,
            ease = freezeFrame.freezeFrameEase
        };
    }



    public override string ToString()
    {
        return string.Format("[{0} damage with '{1}' by '{2}']", damage, team, origin);
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

    public delegate void DelHealthFeedback(Health health);
    public delegate void DelHealthDamageFeedback(DamageInfo damage, Health damaged);
    public delegate void DelHealthModifier(ref DamageInfo damage, Health damaged);
    public event DelHealthModifier OnBeforeDamage;
    public event DelHealthDamageFeedback OnDamage;
    public event DelHealthDamageFeedback OnDeath;
    public event DelHealthFeedback OnMaxHealthChange;

    public virtual int MaxLife
    {
        get
        {
            return _maxLife;
        }
    }

    public virtual int Life
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
            return (float)Life / MaxLife;
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

    protected virtual void Start()
    {
        _lifeNow = MaxLife;
    }

    public bool IsAlive()
    {
        return Life > 0;
    }

    protected virtual void CallMaxHealthChange()
    {
        OnMaxHealthChange?.Invoke(this);
    }

    public static DamageInfo MakeSimpleDamage(int amount, GameObject origin = null, bool unreactable = true, bool ignorePhaseThrough = true, bool feedbacks = true)
    {
        return new DamageInfo()
        {
            damage = amount,
            stunTime = 0f,
            team = DamageTeam.Neutral,
            unreactable = unreactable,
            origin = origin,
            ignorePhaseThrough = ignorePhaseThrough,
            feedbacks = feedbacks
        };
    }

    public virtual DamageResult Damage(DamageInfo info)
    {
        Debug.LogFormat("[DAMAGE] Damage {0} with {1}. Can damage? {2}. Life now is {3} (pha:{4} inv:{5})", this, info, CanDamage(info), Life, PhasingThroughAttacks, Invulnerable);
        if (CanDamage(info))
        {
            if (Invulnerable) return DamageResult.NotDamagingHit;

            OnBeforeDamage?.Invoke(ref info, this);
            int lifeBefore = Life;
            _lifeNow = Mathf.Clamp(Life - info.damage, 0, MaxLife);
            //Debug.LogErrorFormat("{0} is dead? {1} <= 0 so {2}", this, _lifeNow, _lifeNow <= 0);
            OnDamage?.Invoke(info, this);

            TimeManager.Instance.FreezeFrames(info.GetFrameFreeze());

            if (Life <= 0)
            {
                Die(info);
                return DamageResult.KillingHit;
            }


            Debug.LogFormat("[DAMAGE] Now {0} is {1}/{2}", name, Life, MaxLife);
            return GetResult(Life, lifeBefore);

        }
        return DamageResult.Nothing;
    }

    private DamageResult GetResult(int lifeNow, int lifeBefore)
    {
        if (lifeNow > lifeBefore) return DamageResult.HealHit;
        else if (lifeNow < lifeBefore) return DamageResult.DamagingHit;
        else return DamageResult.NotDamagingHit;
    }

    public static bool IsDamage(DamageResult result)
    {
        return result != DamageResult.Nothing && result != DamageResult.HealHit;
    }

    public void Kill(DamageTeam team = DamageTeam.Neutral, GameObject origin = null)
    {
        Damage(new DamageInfo(MaxLife, team, origin));
    }

    protected bool CanDamage(DamageInfo info)
    {
        if (!info.ignorePhaseThrough && PhasingThroughAttacks) return false;
        switch (info.team)
        {
            case DamageTeam.Neutral:
                return true;
            default:
                return info.team == damageFrom;
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
