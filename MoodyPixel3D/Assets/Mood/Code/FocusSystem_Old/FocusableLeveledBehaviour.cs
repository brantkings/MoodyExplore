
using LHH.LeveledBehaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LeveledBehaviour))]
public class FocusableLeveledBehaviour : FocusableObject
{
    LeveledBehaviour _sensor;

    private void Awake()
    {
        _sensor = GetComponent<LeveledBehaviour>();
    }

    protected override void ApplyFocus(int focus)
    {
        _sensor.SetLevel(focus);
    }
}
