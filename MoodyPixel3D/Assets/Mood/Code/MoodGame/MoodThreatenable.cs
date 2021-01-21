using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.LeveledBehaviours.Sensors;

public class MoodThreatenable : MonoBehaviour
{
    public delegate void DelThreatenableEvent(MoodThreatenable affected);

    struct ThreatStruct
    {
        public GameObject threatObject;
        public SensorTarget sensorTarget;

        public ThreatStruct(GameObject obj)
        {
            threatObject = obj;
            sensorTarget = obj.GetComponentInChildren<ISensorTargetGetter>()?.GetTarget();
        }
    }

    public SensorGroup sensorGroup;
    public bool needSensorToThreat;

    private HashSet<ThreatStruct> _threatList;
    public event DelThreatenableEvent OnThreatAppear;
    public event DelThreatenableEvent OnThreatRelief;

    private bool _wasThreatened;

    public void AddThreat(GameObject origin)
    {
        Debug.LogFormat("Add threat {0} to {1}", origin, this);
        if (_threatList == null) _threatList = new HashSet<ThreatStruct>();
        _wasThreatened = IsThreatened();
        _threatList.Add(new ThreatStruct(origin));
        bool isThreatened = IsThreatened();
        if (!_wasThreatened && isThreatened)
        {
            OnThreatAppear?.Invoke(this);
        }
        _wasThreatened = isThreatened;
    }

    public void RemoveThreat(GameObject origin)
    {
        Debug.LogFormat("Remove threat {0} to {1}", origin, this);
        _wasThreatened = IsThreatened();
        _threatList?.RemoveWhere((threatStruct) => threatStruct.threatObject == origin);
        bool isThreatened = IsThreatened();
        if (_wasThreatened && !IsThreatened())
        {
            OnThreatRelief?.Invoke(this);
        }
        _wasThreatened = isThreatened;
    }

    public bool IsThreatened()
    {
        if (!enabled) return false;
        if (_threatList != null)
        {
            foreach (ThreatStruct threat in _threatList)
            {
                if (CanThreat(threat.threatObject) && IsSensing(threat.sensorTarget))
                    return true;
            }
        }

        return false;
    }

    public bool CanThreat(GameObject threat)
    {
        return threat != null && threat.activeSelf;
    }

    public bool IsSensing(SensorTarget target)
    {
        return !needSensorToThreat || target == null || (sensorGroup == null || sensorGroup.IsSensingTarget(target));
    }
}
