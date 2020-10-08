using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Sensors
{
    public class SensorConeOfView : Sensor
    {
        [System.Serializable]
        public struct LevelSetup
        {
            public float maxAngle;
        }

        public Transform leftLineFeedback;
        public Transform rightLineFeedback;
        public SensorLevelSetupList<LevelSetup> levelsSetup;

        LevelSetup _currentInterpolatedLevel;

        protected override void Awake()
        {
            levelsSetup.lerpFunction = (ref LevelSetup lerped, LevelSetup small, LevelSetup big, float factor) =>
            {
                lerped.maxAngle = Mathf.Lerp(small.maxAngle, big.maxAngle, factor);
            };
        }

        public override void SetSensorLevel(float level)
        {
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);

            if(leftLineFeedback != null)
            {
                leftLineFeedback.forward = transform.forward;
                leftLineFeedback.Rotate(0, -_currentInterpolatedLevel.maxAngle, 0);
            }

            if(rightLineFeedback != null)
            {
                rightLineFeedback.forward = rightLineFeedback.forward;
                rightLineFeedback.Rotate(0, _currentInterpolatedLevel.maxAngle, 0);
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var target in SensorTarget.allTargets)
            {
                Vector3 targetPositionProjected = Vector3.ProjectOnPlane(target.transform.position, transform.up);
                Vector3 dir = targetPositionProjected - transform.position;
                float angle = Vector3.Angle(transform.forward, dir);
                
                if (angle < _currentInterpolatedLevel.maxAngle)
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