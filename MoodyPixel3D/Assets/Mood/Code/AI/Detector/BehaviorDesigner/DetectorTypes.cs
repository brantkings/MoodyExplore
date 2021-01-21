using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace BehaviorDesigner
{
    [System.Serializable]
    public class SharedDetector : SharedVariable<Detector>
    {
        public static implicit operator SharedDetector(Detector value) { return new SharedDetector { mValue = value }; }
    }
}