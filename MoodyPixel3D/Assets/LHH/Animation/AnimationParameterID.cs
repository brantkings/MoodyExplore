using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH
{
    [System.Serializable]
    public class AnimationParameterID
    {
        public string parameter;
        private int _parameterID;

        public int GetId()
        {
#if UNITY_EDITOR
            return Animator.StringToHash(parameter);
#else
            if (_parameterID == 0) _parameterID = Animator.StringToHash(parameter);
            return _parameterID;
#endif
        }
    }
}

