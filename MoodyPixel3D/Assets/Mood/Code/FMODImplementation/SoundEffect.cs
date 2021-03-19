using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Linq;
using UnityEngine.Serialization;


public class SoundEffectInstance
{
    public FMOD.Studio.EventInstance instance;

    public static implicit operator FMOD.Studio.EventInstance?(SoundEffectInstance sound)
    {
        if(sound == null) return null;
        else return sound.instance;
    } 

    public static implicit operator SoundEffectInstance(FMOD.Studio.EventInstance sound)
    {
        return new SoundEffectInstance(){instance = sound};
    }

    public PLAYBACK_STATE GetPlayState()
    {
        instance.getPlaybackState(out PLAYBACK_STATE state);
        return state;
    }

    public static bool IsNotNullAndPlaying(SoundEffectInstance sf)
    {
        if(sf == null) return false;
        if(!sf.instance.isValid()) return false;
        
        sf.instance.getPlaybackState(out PLAYBACK_STATE state);

        switch (state)
        {
            case PLAYBACK_STATE.PLAYING:
            return true;
            case PLAYBACK_STATE.STARTING:
            return true;
            case PLAYBACK_STATE.STOPPED:
            return false;
            case PLAYBACK_STATE.STOPPING:
            return true;
            case PLAYBACK_STATE.SUSTAINING:
            return true;
            default:
            return false;
        }
    }

}

public static class SoundEffectInstanceUtils
{
    public static bool IsPlaying(this SoundEffectInstance sf)
    {
        return SoundEffectInstance.IsNotNullAndPlaying(sf);
    }

    public static SoundEffectInstance ExecuteIfNotNull(this SoundEffect sf, Transform where)
    {
        if(sf != null) return sf.InvokeReturn(where);
        else return null;
    }
}

[CreateAssetMenu(menuName = "Long Hat House/Sound/Sound Effect", fileName = "SND_", order = 0)]
public class SoundEffect : ScriptableEvent<SoundEffectInstance>
{
    [FMODUnity.EventRef]
    [FormerlySerializedAs("oneShotString")]
    public string eventString = "event:/Player/New Event";


    private class ParameterInfo
    {
        public string name;

        private FMOD.Studio.PARAMETER_DESCRIPTION description;

        public PARAMETER_ID ID
        {
            get
            {
                return description.id;
            }
        }

        public ParameterInfo(EventDescription eventDescription, string paramName)
        {
            name = paramName;
            eventDescription.getParameterDescriptionByName(paramName, out description);
        }
    }

    [System.Serializable]
    public struct Parameter
    {
        public string name;
        public float value;

        public bool ignoreSeekSpeed;
    }
    

    public FMOD.Studio.PARAMETER_ID[] parameters;

    private FMOD.Studio.EventDescription eventDescription;
    private bool hasEventDescription;


    //private List<FMOD.Studio.EventInstance> instances;

    public bool doNotPlayAtCreation;

    public Parameter[] initialParametersOnStart;

    private Dictionary<string, ParameterInfo> cachedParameters = new Dictionary<string, ParameterInfo>(2);

    [SerializeField]
    private bool _debug;

    public override SoundEffectInstance InvokeReturn(Transform where)
    {
        //Play the sound here
        if(eventString == "") return null;

        FMOD.Studio.EventInstance inst;
#if UNITY_EDITOR
        try
        {
#endif
            inst = FMODUnity.RuntimeManager.CreateInstance(eventString);
#if UNITY_EDITOR
        }
        catch(EventNotFoundException exception)
        {
            Debug.LogWarningFormat("Couldnt find event {0}! ({1})", eventString, exception.Message);
            return null;
        }
#endif
        if(!hasEventDescription)
        {
            hasEventDescription = true;
            inst.getDescription(out eventDescription);
        }

        if (where != null)
        {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(inst, where, where.GetComponentInParent<Rigidbody>());
        }

        foreach(Parameter initialParam in initialParametersOnStart)
        {
            SetParameter(inst, initialParam.name, initialParam.value, initialParam.ignoreSeekSpeed);
        }

        if(!doNotPlayAtCreation)
        {
            inst.start();
        }

        inst.release();

        if(_debug)
        {
            Debug.LogFormat("Playing {0} in position {2} of {3} -> {1}", name, eventDescription, where.position, where.name);
        }


        return new SoundEffectInstance(){instance = inst};
    }

    private ParameterInfo FindCachedParameter(string name)
    {   
        return cachedParameters.ContainsKey(name)? cachedParameters[name] : null;
    }

    private ParameterInfo GetParameter(string name)
    {
        ParameterInfo info = FindCachedParameter(name);
        if(info == null)
        {
            info = new ParameterInfo(eventDescription, name);
            cachedParameters.Add(name, info);
        }

        return info;
    }

    public void SetParameter(SoundEffectInstance inst, string param, float value, bool ignoreSeekSpeed = false)
    {
        SetParameter(inst.instance, param, value, ignoreSeekSpeed);
    }

    public void SetParameter(FMOD.Studio.EventInstance inst, string param, float value, bool ignoreSeekSpeed = false)
    {
        //inst.setParameterByID(GetParameter(param).ID, value, ignoreSeekSpeed);
        inst.setParameterByName(param, value, ignoreSeekSpeed);
        //Debug.LogFormat("Setted parameter {0} to {1} with instance {2}", param, value, inst);
    }

}
