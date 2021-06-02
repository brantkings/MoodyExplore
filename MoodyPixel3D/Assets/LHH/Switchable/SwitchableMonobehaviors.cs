using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{

    public class SwitchableMonobehaviors : SwitchableEffect
    {
        public Behaviour[] toOn;
        public Behaviour[] toOff;

        protected override void Effect(bool on)
        {
            foreach (var bOn in toOn) bOn.enabled = on;
            foreach (var bOff in toOff) bOff.enabled = !on;
        }
    }

}
