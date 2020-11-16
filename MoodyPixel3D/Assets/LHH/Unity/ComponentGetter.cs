using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{
    /// <summary>
    /// Usage: Get() this on Awake or Start and you're good to go!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentGetter<T> where T : Component
    {
        T _stuff;
        public T Get(GameObject obj)
        {
            if (_stuff == null)
            {
                _stuff = obj.GetComponent<T>();
                if (_stuff == null) _stuff = obj.AddComponent<T>();
            }
            return _stuff;
        }

        public T Get()
        {
            return _stuff;
        }

        public static implicit operator T(ComponentGetter<T> obj)
        {
            return obj.Get();
        }
    }
}
