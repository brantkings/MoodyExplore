using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour {

    private List<Health> alreadyDamaged;

    [SerializeField]
    private DamageTeam source = DamageTeam.Enemy;

    [SerializeField]
    private int _amount = 10;

    [SerializeField]
    private bool _onlyDamageOnce;

    private void Awake()
    {
        alreadyDamaged = new List<Health>(2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;

        Health enemy = GetHealth(other);
        

        if (enemy != null)
        {
            DealDamage(enemy);
        }
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

    public virtual void DealDamage(Health health)
    {
        health.Damage(_amount, source);
        if (alreadyDamaged != null) alreadyDamaged.Add(health);
    }
}
