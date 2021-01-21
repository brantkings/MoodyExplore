using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerEvent : RoutineGeneralEvent
{
    [SerializeField]
    private float _time;

    protected override IEnumerator InvokeRoutine()
    {
        yield return new WaitForSeconds(_time);
        CallEvent();
    }
}
