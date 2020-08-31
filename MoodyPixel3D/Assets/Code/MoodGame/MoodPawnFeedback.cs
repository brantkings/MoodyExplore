using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodPawnFeedback : AddonBehaviour<MoodPawn>
{
    public GameObject threatFeedback;
    
    private void OnEnable()
    {
        Addon.OnThreatAppear += OnThreatAppear;
        Addon.OnThreatRelief += OnThreatRelief;
    }
    
    private void OnDisable()
    {
        Addon.OnThreatAppear -= OnThreatAppear;
        Addon.OnThreatRelief -= OnThreatRelief;
    }

    private void OnThreatRelief(MoodPawn pawn)
    {
        OnThreatenedChange(false);
    }

    private void OnThreatAppear(MoodPawn pawn)
    {
        OnThreatenedChange(true);
    }

    private void OnThreatenedChange(bool change)
    {
        if(threatFeedback != null) threatFeedback.SetActive(change);
    }

}
