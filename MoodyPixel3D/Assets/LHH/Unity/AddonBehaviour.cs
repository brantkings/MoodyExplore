using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{
    namespace Core
    {
        public abstract class BehaviourGetter : MonoBehaviour
        {
            protected abstract X Get<X>();
        }

        public abstract class AddonBehaviour_Base<T> : BehaviourGetter
        {

            protected T Addon
            {
                get
                {
                    if (_addon == null) _addon = Get<T>();
#if UNITY_EDITOR
                    if (_addon == null) Debug.LogErrorFormat("{0} has no addon of type {1}.", this, typeof(T));
#endif
                    return _addon;
                }
            }

            private T _addon;
        }

        public abstract class AddonBehaviour_Base<T,U> : AddonBehaviour_Base<T> 
        {

            protected T Addon2
            {
                get
                {
                    if (_addon == null) _addon = Get<T>();
#if UNITY_EDITOR
                    if (_addon == null) Debug.LogErrorFormat("{0} has no addon of type {1}.", this, typeof(T));
#endif
                    return _addon;
                }
            }

            private T _addon;
        }
    }

    public abstract class AddonBehaviour<T> : Core.AddonBehaviour_Base<T> 
    {
        protected override X Get<X>()
        {
            return GetComponent<X>();
        }
    }

    public abstract class AddonBehaviour<T, U> : Core.AddonBehaviour_Base<T, U>
    {
        protected override X Get<X>()
        {
            return GetComponent<X>();
        }
    }

    public abstract class AddonParentBehaviour<T> : Core.AddonBehaviour_Base<T> 
    {
        protected override X Get<X>()
        {
            return GetComponentInParent<X>();
        }
    }

    public abstract class AddonParentBehaviour<T, U> : Core.AddonBehaviour_Base<T, U>
    {
        protected override X Get<X>()
        {
            return GetComponentInParent<X>();
        }
    }
}

