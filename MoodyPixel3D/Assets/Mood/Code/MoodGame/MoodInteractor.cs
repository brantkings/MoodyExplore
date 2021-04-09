using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodInteractor : LHH.Structures.InterfaceTriggerCapture<MoodInteractable>
{
    public bool HasInteractable()
    {
        return Count > 0;
    }
    
    public void Interact()
    {
        MoodInteractable t = GetSelected();
        if (t != null)
        {
            t.Interact(this);
        }
    }
}
