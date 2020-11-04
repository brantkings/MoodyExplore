using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InspectorBasicEvent : MonoBehaviour
{
    public UnityEvent theEvent;

    public virtual void InvokeTheEvent()
    {
        theEvent.Invoke();
    }
}
