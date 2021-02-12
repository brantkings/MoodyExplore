using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{

    public abstract class AddonBehaviour<T> : MonoBehaviour where T:Component
    {
        protected T Addon
        {
            get
            {
                if (_addon == null) _addon = GetComponent<T>();
                return _addon;
            }
        }

        private T _addon;
    }

    public abstract class AddonBehaviour<T, U> : AddonBehaviour<T> where T : Component where U: Component
    {
        protected U Addon2
        {
            get
            {
                if (_addon == null) _addon = GetComponent<U>();
                return _addon;
            }
        }

        private U _addon;
    }
}

