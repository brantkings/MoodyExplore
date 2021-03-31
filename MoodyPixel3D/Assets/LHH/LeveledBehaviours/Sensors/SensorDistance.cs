using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LHH.LeveledBehaviours.Sensors
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

        public Transform minRadiusFeedback;
        public Transform maxRadiusFeedback;

        public SensorLevelSetupList<LevelSetup> levelsSetup;

        LevelSetup _currentInterpolatedLevel;

        protected override void Awake()
        {
            levelsSetup.lerpFunction = (ref LevelSetup lerped, LevelSetup small, LevelSetup big, float factor) => {

                lerped.minDistance = Mathf.Lerp(small.minDistance, big.minDistance, factor);
                lerped.maxDistance = Mathf.Lerp(small.maxDistance, big.maxDistance, factor);
            };
        }

        protected override void ApplyLevel(float level)
        {  
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);

            if(minRadiusFeedback != null)
            {
                minRadiusFeedback.DOKill();
                minRadiusFeedback.DOScale(_currentInterpolatedLevel.minDistance, 0.5f).SetUpdate(true);
            }

            if (maxRadiusFeedback != null)
            {
                maxRadiusFeedback.DOKill();
                maxRadiusFeedback.DOScale(_currentInterpolatedLevel.maxDistance, 0.5f).SetUpdate(true);
            }
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