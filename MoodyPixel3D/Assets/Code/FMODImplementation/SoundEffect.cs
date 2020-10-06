using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Long Hat House/Sound/Sound Effect", fileName = "SND_", order = 0)]
public class SoundEffect : ScriptableEvent
{
    public string oneShotString = "event:/Player/New Event";

    public override void Execute(Transform where)
    {
        //Play the sound here
        FMODUnity.RuntimeManager.PlayOneShotAttached(oneShotString, where.gameObject);
    }

    
}
