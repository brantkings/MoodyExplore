
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours.Sensors
{
    [RequireComponent(typeof(SensorColliderLevelSettings))]
    public class SensorCollider : Sensor
    {
        SensorColliderLevelSettings _levelSettings;

        protected override void Awake()
        {
            _levelSettings = GetComponent<SensorColliderLevelSettings>();
            base.Awake();
        }

        protected override void ApplyLevel(float level)
        {
            _levelSettings.UpdateForSensorLevel(level);
        }

        private void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponent<SensorTarget>();

            if(target != null)
                AddSensorTarget(target);
        }

        private void OnTriggerExit(Collider other)
        {
            var target = other.GetComponent<SensorTarget>();

            if (target != null)
                RemoveSensorTarget(target);
        }
    }
}