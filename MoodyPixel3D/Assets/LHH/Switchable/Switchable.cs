using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{
    public class Switchable : MonoBehaviour {

        public delegate void DelSwitchEvent(bool on);
        public event DelSwitchEvent OnBeforeSwitch;
        public event DelSwitchEvent OnAfterSwitch;

        public SwitchCommand OnStart = SwitchCommand.Off;

        public enum SwitchCommand
        {
            None,
            On,
            Off
        }

        public enum SwitchState
        {
            Undefined,
            Oning,
            On,
            Offing,
            Off
        }

        bool _on;

        protected virtual void Start()
        {
            Set(OnStart, true);
        }

        public void Set(bool on, bool forceEvents = false)
        {
            bool different = on != _on || forceEvents;
            if (different && OnBeforeSwitch != null) OnBeforeSwitch(on);
            _on = on;
            Debug.LogFormat("{0} is now {1}", this, _on);
            if (different && OnAfterSwitch != null) OnAfterSwitch(on);
        }

        public void Set(SwitchCommand state, bool forceEvents = false)
        {
            switch (state)
            {
                case SwitchCommand.None:
                    break;
                case SwitchCommand.On:
                    Set(true, forceEvents);
                    break;
                case SwitchCommand.Off:
                    Set(false, forceEvents);
                    break;
                default:
                    break;
            }
        }

        public bool IsOn()
        {
            return _on;
        }
    }

}
