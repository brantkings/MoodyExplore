using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;

[CreateAssetMenu(menuName = "FMOD/Global Parameter", fileName = "FMOD_GP_")]
public class GlobalSoundParameter : ScriptableObject
{
    public string parameter;

    public float neutralValue;

    private bool initiated;

    public void SetParameter(float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(parameter, value);
    }

    public void ResetParameterToNeutralValue()
    {
        SetParameter(neutralValue);
    }
}
