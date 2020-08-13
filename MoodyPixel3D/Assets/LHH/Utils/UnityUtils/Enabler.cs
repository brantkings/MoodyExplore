using UnityEngine;

namespace LHH.Utils.UnityUtils
{
    [System.Serializable]
    public struct Enabler
    {
        public GameObject[] active;
        public GameObject[] inactive;

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
            Debug.LogFormat("Setting enabler to '{0}'", set);
            foreach (GameObject o in active)
            {
                Debug.LogFormat("Activating {0} {1}", o, Time.frameCount);
                o.SetActive(set);
            }
            foreach (GameObject o in inactive)
            {
                Debug.LogFormat("Deactivating {0} {1}", o, Time.frameCount);
                o.SetActive(!set);
            }
        }
        
    }
}
