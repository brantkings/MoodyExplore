using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchableObjects : SwitchableEffect
{
    public GameObject[] toOn;
    public GameObject[] toOff;

    protected override void Effect(bool on)
    {
        foreach (var bOn in toOn) bOn.SetActive(on);
        foreach (var bOff in toOff) bOff.SetActive(!on);
    }
}
