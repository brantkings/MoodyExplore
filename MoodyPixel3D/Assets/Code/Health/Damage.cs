﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{

    public delegate void DelDamageEvent(Health health, int amount);

    public event DelDamageEvent OnDamage;
    public event DelDamageEvent OnKill;

    private List<Health> alreadyDamaged;

    [SerializeField]
    private DamageTeam source = DamageTeam.Enemy;

    [SerializeField]
    private int _amount = 10;

    [SerializeField]
    private bool _onlyDamageOnce;

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

    private DamageInfo GetDamage()
    {
        return new DamageInfo(_amount, source, gameObject);
    }

    public virtual void DealDamage(Health health)
    {
        health.Damage(GetDamage());
        OnDamage?.Invoke(health, _amount);
        if (!health.IsAlive())
        {
            OnKill?.Invoke(health, _amount);
        }
        if (alreadyDamaged != null) alreadyDamaged.Add(health);
    }
}
