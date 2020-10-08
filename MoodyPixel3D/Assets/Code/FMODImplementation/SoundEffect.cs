using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Linq;

[CreateAssetMenu(menuName = "Long Hat House/Sound/Sound Effect", fileName = "SND_", order = 0)]
public class SoundEffect : ScriptableEvent<FMOD.Studio.EventInstance>
{
    [FMODUnity.EventRef]
    public string oneShotString = "event:/Player/New Event";

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


    //private List<FMOD.Studio.EventInstance> instances;

    public bool doNotPlayAtCreation;

    public Parameter[] initialParametersOnStart;

    private Dictionary<string, ParameterInfo> cachedParameters = new Dictionary<string, ParameterInfo>(2);

    public override FMOD.Studio.EventInstance ExecuteReturn(Transform where)
    {
        //Play the sound here
        FMOD.Studio.EventInstance inst = FMODUnity.RuntimeManager.CreateInstance(oneShotString);

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

        return inst;
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

    public void SetParameter(FMOD.Studio.EventInstance inst, string param, float value, bool ignoreSeekSpeed = false)
    {
        inst.setParameterByID(GetParameter(param).ID, value, ignoreSeekSpeed);
    }
}
