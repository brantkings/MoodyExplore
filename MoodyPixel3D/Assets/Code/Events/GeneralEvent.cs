using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralEvent : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent toCall;

    protected ScriptableEvent[] scriptableEventToCall;

    public void CallEvent()
    {
        toCall.Invoke();
        foreach(ScriptableEvent evt in scriptableEventToCall) evt.Execute(transform);
    }
}
