using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pixel Art Camera/Events/Screen Effect", fileName = "PAEvent_Gradient_")]
public class PixelArtLookUpSettingsOneShot : LHH.ScriptableObjects.Events.ScriptableEvent
{
    [System.Serializable]
    public struct Settings
    {
        public PixelArtLookUpSettingsData data;
        public PixelArtLookUpSettingsGradientChange effect;
        public float duration;
    }

    public Settings[] effects;

    public override void Invoke(Transform where)
    {
        foreach (var ef in effects) ef.effect.DoEffect(ef.data, ef.duration);
    }
}
