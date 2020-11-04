using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterEvent : InspectorBasicEvent
{
    private void OnTriggerEnter(Collider other)
    {
        InvokeTheEvent();
    }
}
