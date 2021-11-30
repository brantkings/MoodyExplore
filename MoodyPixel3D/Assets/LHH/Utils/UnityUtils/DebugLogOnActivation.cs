using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogOnActivation : MonoBehaviour
{
    public UnityEngine.LogType logType = LogType.Error;

    private void OnEnable()
    {
        Debug.unityLogger.Log(logType, message:$"{name} was just activated!", this);
    }

    private void OnDisable()
    {
        Debug.unityLogger.Log(logType, message:$"{name} was just deactivated!", this);
    }
}
