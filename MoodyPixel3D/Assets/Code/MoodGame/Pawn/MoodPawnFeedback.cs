using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodPawnFeedback : AddonBehaviour<MoodPawn>
{
    public GameObject threatFeedback;

    public ScriptableEvent[] onThreatAppear;
    public ScriptableEvent[] onThreatRelax;
    public SoundEffect respirationFeedback;
    public string respirationFeedbackIntensityParameter;
    [Range(0f,100f)]
    public float minValue = 0f;
    [Range(0f,100f)]
    public float maxValue = 100f;
    
    private void OnEnable()
    {
        Addon.OnThreatAppear += OnThreatAppear;
        Addon.OnThreatRelief += OnThreatRelief;

        StartCoroutine(RespirationRoutine());
    
    }
    
    private void OnDisable()
    {
        Addon.OnThreatAppear -= OnThreatAppear;
        Addon.OnThreatRelief -= OnThreatRelief;

        StopAllCoroutines();
    }

    private IEnumerator RespirationRoutine()
    {
        SoundEffectInstance respirationFeedbackInstance = null;
        while(respirationFeedback != null)
        {
            if(!SoundEffectInstance.IsNotNullAndPlaying(respirationFeedbackInstance))
            {
                respirationFeedbackInstance = respirationFeedback.InvokeReturn(Addon.ObjectTransform);
            }

            if(!string.IsNullOrEmpty(respirationFeedbackIntensityParameter))
                respirationFeedback.SetParameter(respirationFeedbackInstance, respirationFeedbackIntensityParameter, GetParameterValue(Addon.GetStaminaRatio()));

            yield return null;
        }
    }

    private float GetParameterValue(float staminaRatio)
    {
        return Mathf.Lerp(minValue, maxValue, 1f - staminaRatio);
    }


    private void OnThreatRelief(MoodPawn pawn)
    {
        OnThreatenedChange(false);
        onThreatRelax.Invoke(pawn.ObjectTransform);
    }

    private void OnThreatAppear(MoodPawn pawn)
    {
        OnThreatenedChange(true);
        onThreatAppear.Invoke(pawn.ObjectTransform);

    }

    private void OnThreatenedChange(bool change)
    {
        if(threatFeedback != null) threatFeedback.SetActive(change);
    }

}
