using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Damage))]
public class DamageEvent : MonoBehaviour
{
    private Damage _damage;

    [Serializable]
    public class DamageUnityEvent : UnityEvent<Transform> {}

    public DamageUnityEvent onConnect;
    public DamageUnityEvent onDamage;
    public DamageUnityEvent onNonDamage;
    public DamageUnityEvent onKill;

    public enum WhereEvent
    {
        ThisTransform,
        HealthTransform
    }

    public WhereEvent where = WhereEvent.HealthTransform;

    private void Awake()
    {
        _damage = GetComponent<Damage>();
    }

    private void OnEnable()
    {
        if(onDamage.HaveCalls()) _damage.OnDamage += OnDamage;  
        if(onKill.HaveCalls()) _damage.OnKill += OnKill;  
    }


    private void OnDisable()
    {
        _damage.OnDamage -= OnDamage;
    }

    private Transform GetWhereFeedback(Health h)
    {
        switch (where)
        {
            case WhereEvent.HealthTransform:
                return h.transform;
            default:
                return transform.transform;
        }
    }

    private void OnDamage(Health health, int amount, Health.DamageResult result)
    {
        Transform where = GetWhereFeedback(health);
        switch (result)
        {
            case Health.DamageResult.DamagingHit:
                onConnect.Invoke(where);
                onDamage.Invoke(where);
                break;
            case Health.DamageResult.NotDamagingHit:
                onConnect.Invoke(where);
                break;
            case Health.DamageResult.KillingHit:
                onConnect.Invoke(where);
                onDamage.Invoke(where);
                break;
            case Health.DamageResult.HealHit:
                onConnect.Invoke(where);
                break;
            default:
                break;
        }
    }
    
    private void OnKill(Health health, int amount, Health.DamageResult result)
    {
        onKill.Invoke(GetWhereFeedback(health));
    }
}
