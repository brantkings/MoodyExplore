using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LHH.Structures
{
    public abstract class InterfaceTriggerCapture<T> : MonoBehaviour where T:class
    {
        private LinkedList<T> _captured;
        
        protected LinkedList<T> Captured
        {
            get { return _captured ??= new LinkedList<T>(); }
        }

        public void OnTriggerEnter(Collider other)
        {
            T b = CaptureThing(other);
            if (b != null)
            {
                Captured.AddFirst(b);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            T b = CaptureThing(other);
            if (b != null)
            {
                Captured.Remove(b);
            }
        }

        protected virtual T CaptureThing(Collider other)
        {
            return other.GetComponentInParent<T>();
        }
    }
}
