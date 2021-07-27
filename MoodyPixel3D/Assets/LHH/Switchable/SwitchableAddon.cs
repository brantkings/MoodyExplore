using LHH.Switchable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SwitchableAddon : MonoBehaviour, LHH.Switchable.ISwitchableAddon
{
    public abstract IEnumerator SwitchSet(bool on, ISwitchableAddon.DelSwitchableAddonEvent onFinish = null);

    public abstract void SwitchSetImmediate(bool on);
}
