using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OrderedActivatedObjects))]
public class OrderedActivatedObjectsOnDetector : AddonBehaviour<OrderedActivatedObjects>
{
    public Detector detector;

    public bool interruptOnUndetect;

    public void OnEnable()
    {
        if(detector != null)
        {
            detector.OnDetect += OnDetect;
            detector.OnUndetect += OnUndetect;
        }
    }

    private void OnDisable()
    {
        if (detector != null)
        {
            detector.OnDetect -= OnDetect;
            detector.OnUndetect -= OnUndetect;
        }
    }

    private void OnDetect()
    {
        Addon.BeginActivation();
    }

    private void OnUndetect()
    {
        Addon.InterruptActivation();
    }
    
}
