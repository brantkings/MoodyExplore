using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTimerEvent : InspectorBasicEvent
{
    [Header("Timer Settings")]
    public float time = 0f;
    public bool unscaled = false;
    public bool startTimerOnEnable = true;

    private Coroutine routine;

    private void OnEnable() 
    {
        if(startTimerOnEnable)
        {
            StartTimer();
        }
    }

    private void OnDisable() {
        if(routine != null) StopCoroutine(routine);
    }

    public void StartTimer()
    {
        routine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        if(unscaled) yield return new WaitForSecondsRealtime(time);
        else yield return new WaitForSeconds(time);
        InvokeTheEvent();
    }
}
