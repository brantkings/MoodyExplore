using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetectorStay : TriggerDetector
{
    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
