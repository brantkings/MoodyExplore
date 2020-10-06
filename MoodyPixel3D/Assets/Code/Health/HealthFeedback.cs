using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFeedback : AddonBehaviour<Health>
{
    public ScriptableEvent[] onDeath;
    public ScriptableEvent[] onDamage;

    private void OnEnable()
    {
        if (Addon != null)
        {
            Addon.OnDamage += OnDamage;
            Addon.OnDeath += OnDeath;
        }
    }

    private void OnDisable()
    {
        if (Addon != null)
        {
            Addon.OnDamage -= OnDamage;
            Addon.OnDeath -= OnDeath;
        }
    }

    private void OnDamage(int amount, Health health)
    {
        onDamage.Execute(transform);
    }

    private void OnDeath(Health health)
    {
        onDeath.Execute(transform);
    }
}
