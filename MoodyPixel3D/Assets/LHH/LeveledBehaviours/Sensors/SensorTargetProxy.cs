
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours.Sensors
{
    public class SensorTargetProxy : MonoBehaviour, ISensorTargetGetter
    {
        SensorTarget _myTarget;

        public SensorTarget GetTarget()
        {
            return _myTarget;
        }

        public void SetTarget(SensorTarget target)
        {
            _myTarget = target;
        }
    }

}
