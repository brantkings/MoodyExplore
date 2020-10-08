using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LHH.Sensors
{
    [RequireComponent(typeof(Collider))]
    public class SensorTarget : MonoBehaviour
    {
        public static LinkedList<SensorTarget> allTargets = new LinkedList<SensorTarget>();

        [System.Serializable]
        public struct SensorTargetEvents
        {
            public UnityEvent onStartedBeingSensed;
            public UnityEvent onStoppedBeingSensed;
        }

        public SensorTargetEvents events;

        HashSet<Sensor> _sensors = new HashSet<Sensor>();
        bool _isBeingSensed = false;

        LinkedListNode<SensorTarget> _allTargetsNode;

        bool _isBeingDisabled = false;

        public void StartBeingSensedBy(Sensor sensor)
        {
            if (_isBeingDisabled || !enabled)
                return;

            _sensors.Add(sensor);
            if (!_isBeingSensed)
            {
                _isBeingSensed = true;
                events.onStartedBeingSensed.Invoke();
            }
        }

        public void StopBeingSensedBy(Sensor sensor)
        {
            if (_isBeingDisabled || !enabled)
                return;

            if (_sensors.Remove(sensor) && _sensors.Count == 0 && _isBeingSensed)
            {
                _isBeingSensed = false;
                events.onStoppedBeingSensed.Invoke();
            }
        }

        private void OnEnable()
        {
            if(_allTargetsNode == null)
                _allTargetsNode = allTargets.AddLast(this);
        }

        private void OnDisable()
        {
            _isBeingDisabled = true;

            if (_allTargetsNode != null)
                allTargets.Remove(_allTargetsNode);

            foreach (var sensor in _sensors)
            {
                sensor.RemoveSensorTarget(this);
            }

            _isBeingDisabled = false;
        }
    }
}