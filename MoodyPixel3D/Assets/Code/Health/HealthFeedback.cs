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

    private void OnDamage(DamageInfo info, Health health)
    {
        onDamage.Invoke(transform);
    }

    private void OnDeath(DamageInfo info, Health health)
    {
        onDeath.Invoke(transform);
    }
}
