using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects.Events;

public class MoodHealthEvents : AddonBehaviour<Health>
{
    public ScriptableListener onDamaged;
    public ScriptableListener onDied;

    private void OnEnable()
    {
        Addon.OnDamage += OnDamage;
        Addon.OnDeath += OnDeath;
    }

    private void OnDamage(DamageInfo damage, Health damaged)
    {
        if (onDamaged != null) 
            onDamaged?.Invoke(transform);
    }
    private void OnDeath(DamageInfo damage, Health damaged)
    {
        if(onDied != null) 
            onDied.Invoke(transform);
    }

}
