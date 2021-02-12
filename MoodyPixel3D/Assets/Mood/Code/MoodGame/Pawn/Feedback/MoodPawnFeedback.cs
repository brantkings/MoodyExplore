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
        Addon.Threatenable.OnThreatAppear += OnThreatAppear;
        Addon.Threatenable.OnThreatRelief += OnThreatRelief;

        StartCoroutine(RespirationRoutine());
    
    }
    
    private void OnDisable()
    {
        Addon.Threatenable.OnThreatAppear -= OnThreatAppear;
        Addon.Threatenable.OnThreatRelief -= OnThreatRelief;

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


    private void OnThreatRelief(MoodThreatenable t)
    {
        Debug.LogFormat("Calling relief on threat! {0}", this);
        OnThreatenedChange(false);
        onThreatRelax.Invoke(Addon.ObjectTransform);
    }

    private void OnThreatAppear(MoodThreatenable t)
    {
        OnThreatenedChange(true);
        onThreatAppear.Invoke(Addon.ObjectTransform);

    }

    private void OnThreatenedChange(bool change)
    {
        if(threatFeedback != null) threatFeedback.SetActive(change);
    }

}
