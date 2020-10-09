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

    public override SoundEffectInstance ExecuteReturn(Transform where)
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
        
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(inst, where, where.GetComponentInParent<Rigidbody>());

        foreach(Parameter initialParam in initialParametersOnStart)
        {
            SetParameter(inst, initialParam.name, initialParam.value, initialParam.ignoreSeekSpeed);
        }

        if(!doNotPlayAtCreation)
        {
            inst.start();
        }

        inst.release();

        return new SoundEffectInstance(){instance = inst};
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

    private ParameterInfo FindCachedParameter(string name)
    {   
        return cachedParameters.ContainsKey(name)? cachedParameters[name] : null;
    }

    public void SetParameter(SoundEffectInstance inst, string param, float value, bool ignoreSeekSpeed = false)
    {
        SetParameter(inst.instance, param, value, ignoreSeekSpeed);
    }

    public void SetParameter(FMOD.Studio.EventInstance inst, string param, float value, bool ignoreSeekSpeed = false)
    {
        Debug.LogFormat("Setting parameter {0} to {1} with instance {2}", param, value, inst);
        inst.setParameterByID(GetParameter(param).ID, value, ignoreSeekSpeed);
    }

}
