using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{
    public class Instantiator<T> : MonoBehaviour where T:Component
    {
        public T prefab;
        public bool thisIsParent;

        public virtual T Instantiate()
        {
            T inst = Instantiate(prefab, transform.position, transform.rotation, thisIsParent? transform : null );
            return inst;
        }
    }
}
