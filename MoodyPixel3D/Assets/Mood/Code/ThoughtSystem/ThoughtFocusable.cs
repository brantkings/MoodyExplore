using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughtFocusable : MonoBehaviour
{
    public int focusablePriority;

    public LHH.Switchable.Switchable focusedSwitchable;
    public LHH.Switchable.Switchable selectedSwitchable;


    #region Interface
    public void SetFocus(bool set)
    {
        focusedSwitchable?.Set(set);
    }

    public void SetSelected(bool set)
    {
        selectedSwitchable?.Set(set);
    }
    #endregion
}
