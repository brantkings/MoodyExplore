using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnterEvent : GeneralEvent
{
    private void OnTriggerEnter(Collider other)
    {
        CallEvent();
    }
}
