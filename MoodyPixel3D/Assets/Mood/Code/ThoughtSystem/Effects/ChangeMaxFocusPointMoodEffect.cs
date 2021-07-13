using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Effect/Max Focus", fileName = "Effect_FocusChange_")]
public class ChangeMaxFocusPointMoodEffect : MoodPawnEffect<ChangeMaxFocusPointMoodEffect.Status>
{

    public int amountAdd = 1;

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

    private void Change(ThoughtSystemController system, int amount)
    {
        while(amount != 0)
        {
            if(amount > 0)
            {
                amount--;
                system.CreateFocusPoint();
            }
            else if (amount < 0)
            {
                amount++;
                system.RemoveFocusPoint();
            }
        }
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

            Change(system, amountAdd);
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

                Change(system, -amountAdd);
            }
        }
    }
}
