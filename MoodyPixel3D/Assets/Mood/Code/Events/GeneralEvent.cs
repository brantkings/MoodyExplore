using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralEvent : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent toCall;

    [SerializeField]
    protected ScriptableEvent[] scriptableEventToCall;

    [SerializeField]
    private LHH.Events.ScriptableListener toListenTo;

    protected virtual void OnEnable()
    {
        if(toListenTo != null)
        {
            toListenTo.Subscribe(OnScriptableEvent);
        }
    }

    protected virtual void OnDisable()
    {

        if (toListenTo != null)
        {
            toListenTo.Unsubscribe(OnScriptableEvent);
        }
    }

    private void OnScriptableEvent()
    {
        CallEvent();
    }


    public void CallEvent()
    {
        toCall.Invoke();
        scriptableEventToCall.Invoke(transform);
    }
}
