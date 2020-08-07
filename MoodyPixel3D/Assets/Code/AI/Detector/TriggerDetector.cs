using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : Detector
{
    protected void OnTriggerEnter(Collider other)
    {
        UpdateTarget(other.transform.root);
        TryUpdateDetecting(true);
    }

    protected void OnTriggerExit(Collider other)
    {
        TryUpdateDetecting(false);
    }
}
