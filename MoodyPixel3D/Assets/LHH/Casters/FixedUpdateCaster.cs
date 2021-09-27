using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LHH.Caster
{
    [RequireComponent(typeof(Caster))]
    public class FixedUpdateCaster : MonoBehaviour
    {
        Caster caster;
        private bool _lastCast = false;
        public event UnityAction<bool> OnCasterStateChange;

        [System.Serializable]
        public class CasterEvent : UnityEvent<bool>
        {

        }

        public CasterEvent onCasterChange;
        public UnityEvent onCasted;
        public UnityEvent onNotCasted;


        private void Awake()
        {
            caster = GetComponent<Caster>();
        }

        private void FixedUpdate()
        {
            bool casted = caster.Cast();
            if(casted != _lastCast)
            {
                OnCasterStateChange?.Invoke(casted);
                onCasterChange.Invoke(casted);
                if (casted) onCasted.Invoke();
                else onNotCasted.Invoke();
            }
            _lastCast = casted;
        }
    }
}