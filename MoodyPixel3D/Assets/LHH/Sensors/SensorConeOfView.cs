using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

        protected override void ApplySensorLevel(float level)
        {
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);

            if(_currentInterpolatedLevel.maxAngle <= 0)
            {
                leftLineFeedback.gameObject.SetActive(false);
                rightLineFeedback.gameObject.SetActive(false);
            }
            else
            {
                leftLineFeedback.gameObject.SetActive(true);
                rightLineFeedback.gameObject.SetActive(true);
            }

            if(leftLineFeedback != null)
            {
                leftLineFeedback.DOKill();
                leftLineFeedback.DOLocalRotate(new Vector3(0, -_currentInterpolatedLevel.maxAngle, 0), .2f, RotateMode.Fast);
            }

            if(rightLineFeedback != null)
            {
                rightLineFeedback.DOKill();
                rightLineFeedback.DOLocalRotate(new Vector3(0, _currentInterpolatedLevel.maxAngle, 0), .2f, RotateMode.Fast);
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