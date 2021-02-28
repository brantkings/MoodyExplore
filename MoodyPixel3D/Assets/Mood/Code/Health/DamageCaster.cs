using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

[RequireComponent(typeof(Caster))]
public class DamageCaster : Damage
{
    Caster caster;

    [Header("Caster")]
    public float timeToRedealDamage = 0f;

    protected override void Awake()
    {
        base.Awake();
        caster = GetComponent<Caster>();
    }

    private void FixedUpdate()
    {
        foreach(RaycastHit hit in caster.CastAll())
        {
            Health health = GetHealth(hit.collider);
            if(health != null)
            {
                DealDamage(health);
            }
        }
    }

    protected override void AddAlreadyDamaged(Health health)
    {
        base.AddAlreadyDamaged(health);
        if (timeToRedealDamage > 0f)
            StartCoroutine(RemoveAlreadyDamagedLater(health, timeToRedealDamage));
    }

    private IEnumerator RemoveAlreadyDamagedLater(Health health, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveAlreadyDamaged(health);
    }
}
