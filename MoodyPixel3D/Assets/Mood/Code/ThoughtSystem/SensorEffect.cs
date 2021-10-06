using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.LeveledBehaviours.Sensors;


[CreateAssetMenu(menuName = "Mood/Effect/Sensor Effect", fileName = "Effect_Sensor_")]
public class SensorEffect : MoodPawnEffect<SensorEffect.SensorEffectStatus>
{
    public struct SensorEffectStatus
    {
        public ThoughtSystemController thought;
        public FocusableObject instance;
    }

    public FocusableObject sensorPrefab;

    protected override SensorEffectStatus? GetInitialStatus(MoodPawn p)
    {
        ThoughtSystemController system = p.GetComponentInChildren<ThoughtSystemController>();
        if (system == null) return null;
        SensorEffectStatus s = new SensorEffectStatus()
        {
            thought = system,
            instance = Instantiate(sensorPrefab, system.thoughtObjectsParent)
        };
        s.instance.GetComponentInParent<SensorGroup>()?.FindSensors();
        s.instance.transform.localPosition = Vector3.zero;
        return s;
    }

    protected override void UpdateStatusAdd(MoodPawn p, ref SensorEffectStatus? status)
    {
        if (status.HasValue)
        {
            status.Value.instance.TryAddOneFocus();
        }
    }

    protected override void UpdateStatusRemove(MoodPawn p, ref SensorEffectStatus? status)
    {
        if (status.HasValue)
        {
            status.Value.instance.TryRemoveOneFocus();
        }
    }
}
