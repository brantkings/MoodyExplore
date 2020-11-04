using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class OnFinishedParticleSystemEvent : InspectorBasicEvent
{
    ParticleSystem system;

    private void Awake()
    {
        system = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        StartCoroutine(OnEnableRoutine());
    }

    IEnumerator OnEnableRoutine()
    {
        yield return new WaitForSeconds(system.main.duration);
        InvokeTheEvent();
    }
}
