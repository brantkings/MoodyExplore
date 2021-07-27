using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Switchable;

[RequireComponent(typeof(Switchable))]
public class DetectorDependentSwitchable : AddonBehaviour<Switchable>
{
    public Detector detector;
    public bool inversed;
    
    private void OnEnable()
    {
        detector.OnChangeDetect += OnChange;
        SetSwitchable(detector.IsDetecting);
    }

    private void OnDisable()
    {
        detector.OnChangeDetect -= OnChange;
    }

    private void OnChange(Detector d, bool detecting)
    {
        SetSwitchable(detecting);
    }

    private void SetSwitchable(bool on)
    {
        Addon.Set(on ^ inversed);
    }

}
