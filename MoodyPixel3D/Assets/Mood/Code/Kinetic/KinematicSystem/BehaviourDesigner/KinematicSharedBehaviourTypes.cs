using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Kinematic
{
    public static class KinematicSharedBehaviourTypes
    {
        [System.Serializable]
        public class SharedKinematicPlatformer : SharedVariable<KinematicPlatformer>
        {
            public static implicit operator SharedKinematicPlatformer(KinematicPlatformer value) { return new SharedKinematicPlatformer { mValue = value }; }
        }
    }
}
