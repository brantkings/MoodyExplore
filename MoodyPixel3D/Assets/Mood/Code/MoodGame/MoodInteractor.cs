using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodInteractor : LHH.Interface.InterfaceTriggerCapture<MoodInteractable>
{


    public enum PriorityValue
    {
        Angle,
        Distance,
    }

    public PriorityValue value;

    public bool HasInteractable()
    {
        return Count > 0;
    }
    
    public void Interact()
    {
        MoodInteractable t = GetSelected();
        if (t != null && t.CanBeInteracted(this))
        {
            t.Interact(this);
        }
    }

    protected override float? GetPriorityValue(MoodInteractable obj)
    {
        if (obj == null) return null;
        Vector3 distance = obj.transform.position - transform.position;
        switch (value)
        {
            case PriorityValue.Angle:
                return Vector3.Angle(Vector3.ProjectOnPlane(distance, Vector3.up), transform.forward);
            case PriorityValue.Distance:
                return Vector3.ProjectOnPlane(distance, Vector3.up).sqrMagnitude;
            default:
                return 0;
        }
    }
}
