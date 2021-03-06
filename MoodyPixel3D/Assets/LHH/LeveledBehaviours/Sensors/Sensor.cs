using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.LeveledBehaviours;

namespace LHH.LeveledBehaviours.Sensors
{

    [System.Serializable]
    public struct SensorLevelSetupList<T>
    {
        [System.Serializable]
        public struct SensorLevelSetup<A>
        {
            public float level;
            public A setup;
        }

        public delegate void SetupLerpFunction(ref T lerped, T small, T big, float factor);

        [SerializeField]
        SensorLevelSetup<T>[] _list;

        public SetupLerpFunction lerpFunction;

        public void CalculateLerpedSetup(float level, ref T lerped)
        {
            if (lerpFunction == null || lerped == null || _list == null || _list.Length == 0)
                return;

            SensorLevelSetup<T> small;
            SensorLevelSetup<T> big;

            if (_list.Length == 1)
            {
                small = _list[0];
                big = _list[0];
                float factor = Mathf.InverseLerp(small.level, big.level, level);

                lerpFunction(ref lerped, small.setup, big.setup, factor);
            }
            else if (_list.Length == 2)
            {
                small = _list[0];
                big = _list[1];
                float factor = Mathf.InverseLerp(small.level, big.level, level);

                lerpFunction(ref lerped, small.setup, big.setup, factor);
            }
            else
            {
                small = _list[0];
                big = _list[0];

                foreach (var current in _list)
                {
                    if (current.level >= level)
                    {
                        big = current;
                        break;
                    }
                    else
                    {
                        small = current;
                    }
                }
                float factor = Mathf.InverseLerp(small.level, big.level, level);

                lerpFunction(ref lerped, small.setup, big.setup, factor);
            }
        }
    }

    public abstract class Sensor : LeveledBehaviour
    {
        public delegate void TargetsListChange();
        public event TargetsListChange OnTargetListChanged;

        public delegate void DetectedTargetChange(SensorTarget target);
        public event DetectedTargetChange OnTargetAdded;
        public event DetectedTargetChange OnTargetRemoved;


        HashSet<SensorTarget> _targets = new HashSet<SensorTarget>();

        protected virtual void Awake()
        {
        }


        public bool AddSensorTarget(SensorTarget target)
        {
            if(_targets.Add(target))
            {
                target.StartBeingSensedBy(this);
                OnTargetAdded?.Invoke(target);
                OnTargetListChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool RemoveSensorTarget(SensorTarget target)
        {
            if (_targets.Remove(target))
            {
                target.StopBeingSensedBy(this);
                OnTargetRemoved?.Invoke(target);
                OnTargetListChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool IsSensingTarget(SensorTarget target)
        {
            return _targets.Contains(target);
        }
    }
}
