using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFeedback : AddonBehaviour<Health>
{
    public InstantiateUtility onDeath;
    public InstantiateUtility onDamage;

    private void OnEnable()
    {
        Addon.OnDamage += OnDamage;
        Addon.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        Addon.OnDamage -= OnDamage;
        Addon.OnDeath -= OnDeath;
    }

    private void OnDamage(int amount, Health health)
    {
        if(onDamage.IsValid()) onDamage.Instantiate(transform);
    }

    private void OnDeath(Health health)
    {
        if(onDeath.IsValid()) onDeath.Instantiate(transform);
    }
}
