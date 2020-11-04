using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAwakeEvent : InspectorBasicEvent
{
    private void Awake()
    {
        InvokeTheEvent();
    }
}
