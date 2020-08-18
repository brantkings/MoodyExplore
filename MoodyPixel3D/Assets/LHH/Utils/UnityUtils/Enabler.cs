using System;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils.UnityUtils
{
    [System.Serializable]
    public struct Enabler
    {
        public GameObject[] active;
        public GameObject[] inactive;
        [SerializeField]
        private ActivationOrder order;

        private enum ActivationOrder
        {
            SimultaneouslyDeactivated,
            SimultaneouslyActivated
        }
        
        
        private bool _setted;
        private bool _set;

        public enum EnabledState
        {
            Active,
            Inactive,
            Unset
        }

        public EnabledState GetState()
        {
            if (!_setted) 
                return EnabledState.Unset;
            else if (_set) 
                return EnabledState.Active;
            else 
                return EnabledState.Inactive;
        }

        public void SetActive(bool set)
        {
            if (_setted && _set == set) 
                return;
            
            _setted = true;
            _set = set;

            switch (order)
            {
                case ActivationOrder.SimultaneouslyActivated:
                    foreach(GameObject o in ToActivate(set)) o.SetActive(true);
                    foreach(GameObject o in ToActivate(!set)) o.SetActive(false);
                    break;
                default:
                    foreach(GameObject o in ToActivate(!set)) o.SetActive(false);
                    foreach(GameObject o in ToActivate(set)) o.SetActive(true);
                    break;
            }
        }

        private IEnumerable<GameObject> ToActivate(bool set)
        {
            if (set) return active;
            else return inactive;
        }

    }
}
