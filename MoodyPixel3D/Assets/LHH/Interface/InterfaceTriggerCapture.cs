using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LHH.Interface
{
    public abstract class InterfaceTriggerCapture<T> : InterfaceCapture<T> where T:class
    {
        public void OnTriggerEnter(Collider other)
        {
            T b = CaptureThing(other);
            AddElement(b);
        }

        public void OnTriggerExit(Collider other)
        {
            T b = CaptureThing(other);
            RemoveElement(b);
        }

        protected virtual T CaptureThing(Collider other)
        {
            return other.GetComponentInParent<T>();
        }

    }
}
