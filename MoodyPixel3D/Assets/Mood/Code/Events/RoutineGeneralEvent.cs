using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoutineGeneralEvent : GeneralEvent
{
    Coroutine routine;

    protected override void OnEnable()
    {
        base.OnEnable();
        AddRoutine();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        RemoveRoutine();
    }


    private void AddRoutine()
    {
        routine = StartCoroutine(InvokeRoutine());
    }

    private void RemoveRoutine()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    protected abstract IEnumerator InvokeRoutine();
}
