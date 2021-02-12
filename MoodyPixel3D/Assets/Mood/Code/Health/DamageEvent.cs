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
    public class DamageUnityEvent : UnityEvent<int> {}

    public DamageUnityEvent onConnect;
    public DamageUnityEvent onDamage;
    public DamageUnityEvent onNonDamage;
    public DamageUnityEvent onKill;

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

    private void OnDamage(Health health, int amount, Health.DamageResult result)
    {
        switch (result)
        {
            case Health.DamageResult.DamagingHit:
                onConnect.Invoke(amount);
                onDamage.Invoke(amount);
                break;
            case Health.DamageResult.NotDamagingHit:
                onConnect.Invoke(amount);
                break;
            case Health.DamageResult.KillingHit:
                onConnect.Invoke(amount);
                onDamage.Invoke(amount);
                break;
            case Health.DamageResult.HealHit:
                onConnect.Invoke(amount);
                break;
            default:
                break;
        }
    }
    
    private void OnKill(Health health, int amount, Health.DamageResult result)
    {
        onKill.Invoke(amount);
    }
}
