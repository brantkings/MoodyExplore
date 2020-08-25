using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class UnityEventUtils
{
    public static bool HaveCalls(this UnityEventBase evt)
    {
        return evt != null && evt.GetPersistentEventCount() > 0;
    }
}
