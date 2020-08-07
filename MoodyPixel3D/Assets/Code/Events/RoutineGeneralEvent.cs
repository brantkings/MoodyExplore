using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoutineGeneralEvent : GeneralEvent
{
    Coroutine routine;

    private void OnEnable()
    {
        routine = StartCoroutine(InvokeRoutine());
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    protected abstract IEnumerator InvokeRoutine();
}
