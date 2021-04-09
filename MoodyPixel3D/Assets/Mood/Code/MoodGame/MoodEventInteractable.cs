using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodEventInteractable : MoodInteractable
{
    public MoodEvent[] whatHappen;
    
    public override void Interact(MoodInteractor interactor)
    {
        foreach (var evt in whatHappen)
        {
            evt.Execute();
        }
    }
    
}
