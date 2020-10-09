using LHH.Sensors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sensor))]
public class FocusableSensor : Focusable
{
    Sensor _sensor;

    private void Awake()
    {
        _sensor = GetComponent<Sensor>();
    }

    protected override void ApplyFocus(int focus)
    {
        _sensor.SetLevel(focus);
    }
}
