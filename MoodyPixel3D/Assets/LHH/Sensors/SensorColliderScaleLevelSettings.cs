using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Sensors
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class SensorColliderScaleLevelSettings : SensorColliderLevelSettings
    {
        [System.Serializable]
        public struct LevelSetup
        {
            public Vector3 scale;
        }

        public SensorLevelSetupList<LevelSetup> levelsSetup;

        LevelSetup _currentInterpolatedLevel;

        protected void Awake()
        {
            levelsSetup.lerpFunction = (ref LevelSetup lerped, LevelSetup small, LevelSetup big, float factor) => {
                lerped.scale = Vector3.Lerp(small.scale, big.scale, factor);
            };
        }

        public override void UpdateForSensorLevel(float level)
        {
            levelsSetup.CalculateLerpedSetup(level, ref _currentInterpolatedLevel);

            transform.localScale = _currentInterpolatedLevel.scale;
        }
    }
}