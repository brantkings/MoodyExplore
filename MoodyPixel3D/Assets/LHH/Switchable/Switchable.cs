using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;

namespace LHH.Switchable
{
    public interface ISwitchableAddon
    {
        public delegate void DelSwitchableAddonEvent(ISwitchableAddon finished);
        /// <summary>
        /// Switch the addon.
        /// </summary>
        /// <param name="on"></param>
        /// <returns></returns>
        IEnumerator SwitchSet(bool on);
        void SwitchSetImmediate(bool on);
    }

    public class Switchable : MonoBehaviour {

        public class WaitForSwitchable : CustomYieldInstruction
        {
            Switchable s;
            Switchable.SwitchState st;

            public WaitForSwitchable(Switchable sw, Switchable.SwitchState stateToWaitFor)
            {
                s = sw; st = stateToWaitFor;
            }

            public override bool keepWaiting => s.GetState() != st;
        }

        public delegate void DelSwitchEvent(bool on);
        public event DelSwitchEvent OnBeforeSwitch;
        public event DelSwitchEvent OnAfterSwitch;

        public enum Order
        {
            AlwaysSame,
            InvertIfOff,
        }

        public enum WaitStyle
        {
            AllAtOnce,
            OneAtATime,
            NeverWait
        }

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

        public SwitchCommand OnStart = SwitchCommand.Off;
        public bool findAddonsOnChildren = true;
        public WaitStyle howToWaitAddons = WaitStyle.AllAtOnce;
        public Order orderAddons = Order.AlwaysSame;
#if UNITY_EDITOR
        [SerializeField]
        protected bool switchableDebug;
#endif

        [SerializeField]
        [ReadOnly]
        private SwitchState _state;
        private List<ISwitchableAddon> _addons;
        private Coroutine _currentRoutine;

        private void Awake()
        {
            FindAddons(findAddonsOnChildren);
        }

        private void FindAddons(bool findChildren)
        {
            ISwitchableAddon[] addons = findChildren ? gameObject.GetComponentsInChildren<ISwitchableAddon>(true) : gameObject.GetComponents<ISwitchableAddon>();
            _addons = new List<ISwitchableAddon>(addons);
#if UNITY_EDITOR
            if(switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} There are {1} addons, +{2}+", this, addons.Length, GetAddonsString(_addons));
#endif
        }

        private string GetAddonsString(IEnumerable list)
        {
            string r = "";
            bool first = false;
            foreach (var a in list) {
                if (first) r += ", ";
                r += "[" + a.ToString() + "]";
                first = true;
            }
            return r;
        }

        public void AddAddon(ISwitchableAddon addon)
        {
            _addons.Add(addon);
        }

        public void RemoveAddon(ISwitchableAddon addon)
        {
            _addons.Remove(addon);
        }

        protected virtual void Start()
        {
            if(_state == SwitchState.Undefined) Set(OnStart, true, true);
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if(_currentRoutine != null)
            {
                Debug.LogWarningFormat("[SWITCHABLE] Disabling {0} with a coroutine!. Object is active? {1}", this, gameObject.activeInHierarchy);
            }
#endif
        }

        public void Set(bool on)
        {
            Set(on, false, false);
        }

        public void SetImmediate(bool on)
        {
            Set(on, false, true);
        }

        public void Set(bool on, bool forceEvents, bool immediate)
        {
            if (_currentRoutine != null)
            {
                //Debug.LogWarningFormat(this, "[SWITCHABLE] Coroutine is not null on {0}", this);
                //StopCoroutine(_currentRoutine);
                StopAllCoroutines();
            }
            _currentRoutine = StartCoroutine(SetRoutine(on, forceEvents, immediate, this));
        }

        private IEnumerator SetRoutine(bool on, bool forceEvents, bool immediate, MonoBehaviour coroutineMaster)
        {
            bool? isOn = IsOn();
            bool different = on != isOn || forceEvents;
            SetState(on, false);
            if (different && OnBeforeSwitch != null) OnBeforeSwitch(on);
            if (immediate)
            {
                foreach (var addon in _addons)
                {
#if UNITY_EDITOR
                    if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Setting {1} as 'on:{2}' immediately. [{3}]", this, addon, on, Time.time);
#endif
                    addon.SwitchSetImmediate(on);
                }
            }
            else
            {
                switch (howToWaitAddons)
                {
                    case WaitStyle.AllAtOnce:
                        HashSet<ISwitchableAddon> waitingToEnd = new HashSet<ISwitchableAddon>(_addons);
                        ISwitchableAddon.DelSwitchableAddonEvent onFinish = (x) =>
                        {

                            waitingToEnd.Remove(x);
    #if UNITY_EDITOR
                            if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Waited for {1} as 'on:{2}'! Still need {3} will wait for all. [{4}]", this, x, on, waitingToEnd.Count, Time.time);
    #endif
                        };
                        foreach (var addon in _addons)
                        {
    #if UNITY_EDITOR
                            if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Setting {1} as 'on:{2}'. Gonna wait for all. [{3}]", this, addon, on, Time.time);
    #endif
                            coroutineMaster.StartCoroutine(WaitForCoroutine(addon.SwitchSet(on), addon, onFinish));
                        }
                        if(waitingToEnd.Count > 0)
                        {
                            yield return new WaitWhile(() => waitingToEnd.Count > 0);
                        }
                        waitingToEnd = null;
                        break;
                    case WaitStyle.OneAtATime:
                        foreach (var addon in _addons)
                        {
    #if UNITY_EDITOR
                            if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Setting {1} as 'on:{2}'. Waiting. [{3}]", this, addon, on, Time.time);
    #endif
                            yield return addon.SwitchSet(on);
                        }
                        break;
                    case WaitStyle.NeverWait:
                        foreach (var addon in _addons)
                        {
    #if UNITY_EDITOR
                            if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Setting {1} as 'on:{2}'. Not waiting. [{3}]", this, addon, on, Time.time);
    #endif
                            yield return coroutineMaster.StartCoroutine(addon.SwitchSet(on));
                        }
                        break;
                    default:
                        break;
                }
            }


            SetState(on, true); //This has a debug log
            if (different && OnAfterSwitch != null) OnAfterSwitch(on);
            _currentRoutine = null;
        }

        private IEnumerator WaitForCoroutine(IEnumerator routine, ISwitchableAddon addon, ISwitchableAddon.DelSwitchableAddonEvent onEnd)
        {
            yield return routine;
            onEnd?.Invoke(addon);
        }

        private IEnumerable<ISwitchableAddon> GetAddons(bool on)
        {
            switch (orderAddons)
            {
                case Order.InvertIfOff:
                    if (on) return _addons;
                    else return _addons.Reverse<ISwitchableAddon>();
                default:
                    return _addons;
            }
        }

        public void Set(SwitchCommand state)
        {
            Set(state, false, false);
        }

        public void SetImmediate(SwitchCommand state)
        {
            Set(state, false, true);
        }

        public void Set(SwitchCommand state, bool forceEvents, bool immediate)
        {
            switch (state)
            {
                case SwitchCommand.None:
                    break;
                case SwitchCommand.On:
                    Set(true, forceEvents, immediate);
                    break;
                case SwitchCommand.Off:
                    Set(false, forceEvents, immediate);
                    break;
                default:
                    break;
            }
        }

        public bool? IsOn()
        {
            switch (GetState())
            {
                case SwitchState.Undefined:
                    return null;
                case SwitchState.Oning:
                    return true;
                case SwitchState.On:
                    return true;
                case SwitchState.Offing:
                    return false;
                case SwitchState.Off:
                    return false;
                default:
                    return null;
            }
        }

        private SwitchState GetCurrentState(bool on, bool complete)
        {
            if(on)
            {
                if (complete) return SwitchState.On;
                else return SwitchState.Oning;
            }
            else
            {
                if (complete) return SwitchState.Off;
                else return SwitchState.Offing;
            }
        }

        public SwitchState GetState()
        {
            return _state;
        }

        private void SetState(SwitchState state)
        {
            _state = state;
#if UNITY_EDITOR
            if (switchableDebug) Debug.LogFormat(this, "[SWITCHABLE] {0} Setting state '{1}'. [{2}]", this, state, Time.time);
#endif
        }

        private void SetState(bool on, bool completed)
        {
            SetState(GetCurrentState(on, completed));
        }
    }

}
