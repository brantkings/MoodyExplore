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
        scriptableEventToCall.Invoke(transform);
    }
}
