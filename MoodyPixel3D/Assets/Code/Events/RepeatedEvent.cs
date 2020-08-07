using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatedEvent : RoutineGeneralEvent
{
    public float waitBeforeFirstInvoke = 0f;
    public float timeBetweenInvokes = 1f;
    
    protected override IEnumerator InvokeRoutine()
    {
        if(waitBeforeFirstInvoke > 0)
            yield return new WaitForSeconds(waitBeforeFirstInvoke);
        while(true)
        {
            CallEvent();
            yield return new WaitForSeconds(timeBetweenInvokes);
        }
    }
}
