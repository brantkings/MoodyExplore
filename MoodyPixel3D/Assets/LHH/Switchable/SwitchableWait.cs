using LHH.Switchable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchableWait : SwitchableAddon
{
    public float allWaitsAdd;
    public float waitOning;
    public float waitOffing;
    public bool unscaled;

    public override IEnumerator SwitchSet(bool on, ISwitchableAddon.DelSwitchableAddonEvent onFinish = null)
    {
        if (unscaled) yield return new WaitForSecondsRealtime((on? waitOning : waitOffing) + allWaitsAdd);
        else yield return new WaitForSeconds((on ? waitOning : waitOffing) + allWaitsAdd);
    }

    public override void SwitchSetImmediate(bool on)
    {
    }
}
