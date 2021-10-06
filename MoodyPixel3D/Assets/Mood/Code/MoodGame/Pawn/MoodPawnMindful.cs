using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.LeveledBehaviours.Sensors;


public interface IMindPawn
{
    int GetMaxFocusPoints();

    int GetAvailableFocusPoints();
}

public interface IFocusPointController
{
    int MaxPoints { get; }
    int AvailablePoints { get; }

    void SetPain(int pain);
}

public class MoodPawnMindful : MoodPawn, IMindPawn
{
    [Header("Focusable")]
    private IFocusPointController _pointController;
    public IFocusPointController PointController
    {
        get
        {
            if (_pointController == null) _pointController = GetComponentInChildren<IFocusPointController>();
            return _pointController;
        }
    }
    public SensorGroup sensorGroup;

    public int GetMaxFocusPoints()
    {
        return PointController.MaxPoints;
    }

    public int GetAvailableFocusPoints()
    {
        return PointController.AvailablePoints;
    }

    public override bool IsSensing(SensorTarget target)
    {
        Debug.LogErrorFormat("Is {0} sensing {1}? {2} || {3}", this, target, sensorGroup.IsSensingTarget(target), base.IsSensing(target));
        return sensorGroup.IsSensingTarget(target) || base.IsSensing(target);
    }

    protected override void OnDamage(DamageInfo info, Health health)
    {
        base.OnDamage(info, health);
        PointController.SetPain((health.MaxLife - health.Life) / 10);
    }
}
