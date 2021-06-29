using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        s.instance.transform.localPosition = Vector3.zero;
        Debug.LogFormat("{0} instantiated {1} for {2}.", this, s.instance, p);
        return s;
    }

    protected override void UpdateStatusAdd(MoodPawn p, ref SensorEffectStatus? status)
    {
        Debug.LogFormat("{0} updating add {1} for {2}.", this, status?.instance, p);
        if (status.HasValue)
        {
            status.Value.instance.TryAddOneFocus();
        }
    }

    protected override void UpdateStatusRemove(MoodPawn p, ref SensorEffectStatus? status)
    {
        Debug.LogFormat("{0} updating remove {1} for {2}.", this, status?.instance, p);
        if (status.HasValue)
        {
            status.Value.instance.TryRemoveOneFocus();
        }
    }
}
