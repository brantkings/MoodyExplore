using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Sensors
{
    public class SensorGroup : MonoBehaviour
    {
        public bool applyLevelToSubSensors = false;

        public Sensor[] _group;

        public bool IsSensingTarget(SensorTarget target)
        {
            foreach (var sensor in _group)
            {
                if (sensor.IsSensingTarget(target))
                    return true;
            }

            return false;
        }
    }
}