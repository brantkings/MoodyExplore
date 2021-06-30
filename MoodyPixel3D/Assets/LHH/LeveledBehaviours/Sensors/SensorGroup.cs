
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours.Sensors
{
    public class SensorGroup : MonoBehaviour
    {
        public bool applyLevelToSubSensors = false;

        public Sensor[] _group;

        public void FindSensors()
        {
            _group = GetComponentsInChildren<Sensor>();
        }

        public bool IsSensingTarget(SensorTarget target)
        {
            if(target != null)
            {
                foreach (var sensor in _group)
                {
                    if (sensor != null && sensor.IsSensingTarget(target))
                        return true;
                }
            }

            return false;
        }
    }
}