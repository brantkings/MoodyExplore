using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.LeveledBehaviours.Sensors;


public interface IMindPawn
{
    int GetMaxFocusPoints();

    int GetAvailableFocusPoints();
}

public class MoodPawnMindful : MoodPawn, IMindPawn
{ 
    [Header("Focusable")]
    public FocusController focus;
    public SensorGroup sensorGroup;

    public int GetMaxFocusPoints()
    {
        return focus.MaxPoints;
    }

    public int GetAvailableFocusPoints()
    {
        return focus.AvailablePoints;
    }

    public override bool IsSensing(SensorTarget target)
    {
        return sensorGroup.IsSensingTarget(target) || base.IsSensing(target);
    }

    protected override void OnDamage(DamageInfo info, Health health)
    {
        base.OnDamage(info, health);
        if (focus != null)
        {
            Debug.LogFormat("Setting pain as {0} - {1} = {2}", health.MaxLife, health.Life, health.MaxLife - health.Life);
            focus.SetPain((health.MaxLife - health.Life) / 10);
        }
    }
}
