using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodThreat : MonoBehaviour
{
    HashSet<MoodPawn> _threatened = new HashSet<MoodPawn>();

    private void OnTriggerEnter(Collider other)
    {
        MoodPawn p = other.GetComponentInParent<MoodPawn>();
        if(p != null)
        {
            if(_threatened.Add(p))
            {
                p.AddThreat(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MoodPawn p = other.GetComponentInParent<MoodPawn>();
        if (p != null)
        {
            if(_threatened.Remove(p))
            {
                p.RemoveThreat(gameObject);
            }
        }
    }

    private void OnEnable()
    {
        foreach(MoodPawn p in _threatened)
        {
            p.AddThreat(gameObject);
        }
    }

    private void OnDisable()
    {
        foreach (MoodPawn p in _threatened)
        {
            p.RemoveThreat(gameObject);
        }
    }
}
