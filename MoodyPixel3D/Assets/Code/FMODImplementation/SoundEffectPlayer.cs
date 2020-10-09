using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    public SoundEffect sfx;

    private SoundEffectInstance latestInstance;

    public FMOD.Studio.STOP_MODE howToStop = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

    public SoundEffectInstance Play()
    {
        latestInstance = sfx.ExecuteReturn(transform);
        return latestInstance;
    }

    public void Stop()
    {
        if(latestInstance.IsPlaying()) latestInstance.instance.stop(howToStop);
    }
}
