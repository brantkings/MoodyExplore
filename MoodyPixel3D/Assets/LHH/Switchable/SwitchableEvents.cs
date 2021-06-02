using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LHH.Switchable
{
    public class SwitchableEvents : SwitchableEffect
    {
        public UnityEvent on;
        public UnityEvent off;

        protected override void Effect(bool isOn)
        {
            if (isOn) this.on.Invoke();
            else this.off.Invoke();
        }
    }

}
