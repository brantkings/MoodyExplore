using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaxFocusPointMoodEffect : MoodPawnEffect<ChangeMaxFocusPointMoodEffect.Status>
{

    public struct Status
    {
        public int amountAdded;
    }

    protected override Status? GetInitialStatus(MoodPawn p)
    {
        return new Status()
        {
            amountAdded = 0
        };
    }

    protected override void UpdateStatusAdd(MoodPawn p, ref Status? status)
    {
        if (status.HasValue)
        {
            ThoughtSystemController system = p.GetComponentInChildren<ThoughtSystemController>();

            if (system == null) status = GetInitialStatus(p);
            Status s = status.Value;
            s.amountAdded++;
            status = s;

            system.CreateFocusPoint();
        }
    }

    protected override void UpdateStatusRemove(MoodPawn p, ref Status? status)
    {
        if (status.HasValue)
        {
            ThoughtSystemController system = p.GetComponentInChildren<ThoughtSystemController>();

            if(system != null)
            {
                Status s = status.Value;
                s.amountAdded--;

                if (s.amountAdded > 0)
                {
                    status = s;
                }
                else
                {
                    status = null;
                }

                system.RemoveFocusPoint();
            }
        }
    }
}
