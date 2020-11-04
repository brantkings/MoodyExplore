using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDisableEvent : InspectorBasicEvent
{
    private void OnDisable()
    {
        InvokeTheEvent();
    }
}
