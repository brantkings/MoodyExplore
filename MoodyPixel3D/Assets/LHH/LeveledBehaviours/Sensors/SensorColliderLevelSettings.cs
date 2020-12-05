using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.LeveledBehaviours.Sensors
{

    public abstract class SensorColliderLevelSettings : MonoBehaviour
    {
        public abstract void UpdateForSensorLevel(float level);
    }

}