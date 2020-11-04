using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableEvent : InspectorBasicEvent
{
    private void OnEnable()
    {
        InvokeTheEvent();
    }
}
