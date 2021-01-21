using LHH.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Damage : MonoBehaviour
{

    public delegate void DelDamageEvent(Health health, int amount);

    public event DelDamageEvent OnDamage;
    public event DelDamageEvent OnKill;

    private List<Health> alreadyDamaged;

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
    private float _stunTime = 0.5f;

    [SerializeField]
    private bool _onlyDamageOnce;

    [Space()]
    public MorphableProperty<KnockbackSolver> knockback;

    [Space()]
    [SerializeField]
    private bool _debug;



    private void Awake()
    {
        alreadyDamaged = new List<Health>(2);
    }

    public void SetSourceDamageTeam(DamageTeam newTeam)
    {
        source = newTeam;
    }

    public Vector3 GetDirection(Transform target)
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

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;

        Health enemy = GetHealth(other);
        

        if (enemy != null)
        {
            DealDamage(enemy);
        }
        #if UNITY_EDITOR
        if (_debug)
        {
            Debug.LogFormat(
                "{0} entered {1} and it is going to damage {2} by {3} damage. It already damaged how many? {4}", 
                this, other, enemy, _amount, alreadyDamaged.Count);
        }
        #endif
    }

    private void OnTriggerExit(Collider other)
    {
        if (_onlyDamageOnce) return;

        Health enemy = GetHealth(other);
        if (enemy != null)
        {
            alreadyDamaged.Remove(enemy);
        }
    }

    private Health GetHealth(Collider other)
    {
        Transform chain = other.transform;
        while(chain != null)
        {
            Health h = chain.GetComponent<Health>();
            if (h != null) return h;
            Damage d = chain.GetComponent<Damage>();
            if (d != null) return null; //Dont ascend through the chain anymore.
            chain = chain.parent;
        }
        return null;
    }

    private DamageInfo GetDamage(Transform target)
    {
        return new DamageInfo(_amount, source, gameObject).SetDirection(GetDirection(target)).SetForce(knockback.Get().GetKnockback(transform, target, out float angle), angle, knockback.Get().GetDuration()).SetStunTime(_stunTime);
    }


    public virtual void DealDamage(Health health)
    {
        health.Damage(GetDamage(health.transform));
        OnDamage?.Invoke(health, _amount);
        if (!health.IsAlive())
        {
            OnKill?.Invoke(health, _amount);
        }
        if (alreadyDamaged != null) alreadyDamaged.Add(health);
    }
}
