using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Sensors
{

    public class SensorDistance : Sensor
    {
        private void OnDrawGizmos()
        {
            Color color = Color.red;
            color.a = .3f;
            Gizmos.DrawWireSphere(transform.position, _currentInterpolatedLevel.minDistance);

            color = Color.blue;
            color.a = .3f;
            Gizmos.DrawWireSphere(transform.position, _currentInterpolatedLevel.maxDistance);
        }

        [System.Serializable]
        public struct LevelSetup
        {
            public float minDistance;
            public float maxDistance;
        }

        public SensorLevelSetupList<LevelSetup> levelsSetup;

        LevelSetup _currentInterpolatedLevel;

        protected override void Awake()
        {
            levelsSetup.lerpFunction = (ref LevelSetup a, LevelSetup small, LevelSetup big, float factor) => {

                a.minDistance = Mathf.Lerp(small.minDistance, big.minDistance, factor);
                a.maxDistance = Mathf.Lerp(small.maxDistance, big.maxDistance, factor);
            };
        }

        public override void SetSensorLevel(float level)
        {  
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var target in SensorTarget.allTargets)
            {
                float dist = Vector3.Distance(target.transform.position, transform.position);
                if (dist > _currentInterpolatedLevel.minDistance && dist < _currentInterpolatedLevel.maxDistance)
                {
                    AddSensorTarget(target);
                }
                else
                {
                    RemoveSensorTarget(target);
                }
            }
        }
    }

}