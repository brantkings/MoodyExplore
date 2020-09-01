using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LHH.Structures
{

    public class InterfaceCaptureFeedback<T> : MonoBehaviour where T:class
    {
        public InterfaceCapture<T> capture;

        [System.Serializable]
        public class TEvent : UnityEngine.Events.UnityEvent<T>
        {

        }

        public TEvent onChange;

        private void Awake()
        {
            capture = GetComponent<InterfaceCapture<T>>();
        }

        private void OnEnable()
        {
            OnChange(capture.GetSelected());
            capture.OnChangeSelected += OnChange;
        }

        private void OnDisable()
        {
            capture.OnChangeSelected -= OnChange;
        }

        protected virtual void OnChange(T newfirst)
        {
            onChange.Invoke(newfirst);
        }
    }
}
