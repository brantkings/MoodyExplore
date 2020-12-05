using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours.Sensors
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class SensorColliderCapsuleLevelSettings : SensorColliderLevelSettings
    {
        [System.Serializable]
        public struct LevelSetup
        {
            public float radius;
        }

        public SensorLevelSetupList<LevelSetup> levelsSetup;

        CapsuleCollider _col;
        LevelSetup _currentInterpolatedLevel;

        protected void Awake()
        {
            _col = GetComponent<CapsuleCollider>();

            levelsSetup.lerpFunction = (ref LevelSetup lerped, LevelSetup small, LevelSetup big, float factor) => {
                lerped.radius = Mathf.Lerp(small.radius, big.radius, factor);
            };
        }

        public override void UpdateForSensorLevel(float level)
        {
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);

            _col.radius = _currentInterpolatedLevel.radius;
        }
    }
}