using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Unity;

public class Damage : MonoBehaviour
{
    public delegate void DelDamageEvent(Health health, int amount, Health.DamageResult result);

    public event DelDamageEvent OnDamage;
    public event DelDamageEvent OnKill;

    private HashSet<Health> alreadyDamaged;

    public enum DirectionStyle
    {
        Forward,
        PositionDifference
    }

    [SerializeField]
    private DamageTeam source = DamageTeam.Enemy;


    [SerializeField]
    private DirectionStyle direction = DirectionStyle.PositionDifference;

    [SerializeField]
    private int _amount = 10;

    [SerializeField]
    private TimeBeatManager.BeatQuantity _stunTime = 4;

    [SerializeField]
    private bool _ignorePhaseThrough;

    [SerializeField]
    private bool _onlyDamageOnce;



    [Space()]
    public MorphableProperty<KnockbackSolver> knockback;


    protected virtual void Awake()
    {
        alreadyDamaged = new HashSet<Health>();
    }

    protected virtual void OnEnable()
    {
        FlushAlreadyDamaged();
    }

    public void SetSourceDamageTeam(DamageTeam newTeam)
    {
        source = newTeam;
    }

    protected Vector3 GetDirection(Transform target)
    {
        switch (direction)
        {
            case DirectionStyle.Forward:
                return transform.forward;
            case DirectionStyle.PositionDifference:
                return target.position - transform.position;
            default:
                return target.position - transform.position;
        }
    }

    public DamageTeam Team
    {
        get
        {
            return source;
        }
    }



    protected Health GetHealth(Collider other)
    {
        Transform chain = other.transform;
        while (chain != null)
        {
            Health h = chain.GetComponent<Health>();
            if (h != null) return h;
            Damage d = chain.GetComponent<Damage>();
            if (d != null) return null; //Dont ascend through the chain anymore.
            chain = chain.parent;
        }
        return null;
    }

    protected DamageInfo GetDamage(Transform target)
    {
        return new DamageInfo(_amount, source, gameObject)
            .SetDirection(GetDirection(target))
            .SetForce(knockback.Get()
            .GetKnockback(transform, target, out float angle), angle, knockback.Get().GetDuration())
            .SetStunTime(_stunTime)
            .SetIgnorePhaseThrough(_ignorePhaseThrough);
    }


    public virtual void DealDamage(Health health)
    {
        if (health == null)
        {
            OnDamage?.Invoke(null, _amount, Health.DamageResult.NotDamagingHit);
            return;
        }
        if (AlreadyDamaged(health)) return;
        Health.DamageResult result = health.Damage(GetDamage(health.transform));
        OnDamage?.Invoke(health, _amount, result);
        if (!health.IsAlive())
        {
            OnKill?.Invoke(health, _amount, result);
        }
        AddAlreadyDamaged(health);
    }

    protected bool AlreadyDamaged(Health health)
    {
        if (alreadyDamaged != null) return alreadyDamaged.Contains(health);
        else return false;
    }

    protected virtual void AddAlreadyDamaged(Health health)
    {
        if (alreadyDamaged != null) alreadyDamaged.Add(health);
    }

    protected virtual void RemoveAlreadyDamaged(Health health)
    {
        if (_onlyDamageOnce) return;
        if (alreadyDamaged != null) alreadyDamaged.Remove(health);
    }

    protected void RemoveAlreadyDamaged(Collider collider)
    {
        if (_onlyDamageOnce) return;
        RemoveAlreadyDamaged(GetHealth(collider));
    }

    protected void FlushAlreadyDamaged()
    {
        if (alreadyDamaged != null) alreadyDamaged.Clear();
    }

    protected int GetAlreadyDamagedLength()
    {
        if (alreadyDamaged != null) return alreadyDamaged.Count;
        else return 0;
    }
}
