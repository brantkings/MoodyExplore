using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodInteractable : MonoBehaviour
{
    public MoodEvent[] whatHappen;
    
    public void Execute()
    {
        foreach (var evt in whatHappen)
        {
            evt.Execute();
        }
    }
    
}
