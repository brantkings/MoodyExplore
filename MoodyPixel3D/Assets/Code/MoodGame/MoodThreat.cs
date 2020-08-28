using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodThreat : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponentInParent<MoodPawn>()?.AddThreat(gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponentInParent<MoodPawn>()?.RemoveThreat(gameObject);
    }
}
