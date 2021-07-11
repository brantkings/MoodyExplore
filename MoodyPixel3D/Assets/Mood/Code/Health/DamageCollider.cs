using LHH.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageCollider : Damage
{
    [Space()]
    [SerializeField]
    private bool _invokeEventsOnlyWithValidHealth;

    [Space()]
    [SerializeField]
    private bool _debug;



    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;

        Health enemy = GetHealth(other);


        if (!_invokeEventsOnlyWithValidHealth || enemy != null)
        {
            DealDamage(enemy);
        }
#if UNITY_EDITOR
        if (_debug)
        {
            DamageInfo info = GetDamage(other.transform);
            Debug.LogFormat(
                "{0} entered {1} and it is going to damage {2} by {3} damage. It already damaged how many? {4}",
                this, other, enemy, info.damage, GetAlreadyDamagedLength());
        }
#endif
    }


    private void OnTriggerExit(Collider other)
    {
        RemoveAlreadyDamaged(other);
    }
}
