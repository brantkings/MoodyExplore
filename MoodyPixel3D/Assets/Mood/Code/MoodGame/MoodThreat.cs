using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodThreat : MonoBehaviour
{
    HashSet<MoodThreatenable> _threatened = new HashSet<MoodThreatenable>();

    private void OnTriggerEnter(Collider other)
    {
        MoodThreatenable p = other.GetComponentInParent<MoodThreatenable>();
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
        MoodThreatenable p = other.GetComponentInParent<MoodThreatenable>();
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
        foreach(MoodThreatenable p in _threatened)
        {
            p.AddThreat(gameObject);
        }
    }

    private void OnDisable()
    {
        foreach (MoodThreatenable p in _threatened)
        {
            p.RemoveThreat(gameObject);
        }
    }
}
