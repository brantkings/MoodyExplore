using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{

    [RequireComponent(typeof(Switchable))]
    public abstract class SwitchableEffect : MonoBehaviour {

        protected Switchable _switch;
        public bool _debugEffect;

        protected virtual void Awake()
        {
            _switch = GetComponent<Switchable>();
        }

        private void OnEnable()
        {
            _switch.OnAfterSwitch += Effect;
#if UNITY_EDITOR
            if (_debugEffect)
            {
                Debug.LogFormat("[SWITCHABLE EFFECT] {0} added to {1}.", this, _switch);
                _switch.OnAfterSwitch += DebugEffect;
            }
#endif
        }


        private void OnDisable()
        {
            _switch.OnAfterSwitch -= Effect;
#if UNITY_EDITOR
            if (_debugEffect)
            {
                Debug.LogFormat("[SWITCHABLE EFFECT] {0} removed to {1}.", this, _switch);
                _switch.OnAfterSwitch -= DebugEffect;
            }
#endif
        }

        abstract protected void Effect(bool on);

        protected virtual void DebugEffect(bool on)
        {
            UnityEngine.Debug.LogFormat("[SWITCHABLE EFFECT] {0} is now {1}!", this, on);
        }
    }

}
