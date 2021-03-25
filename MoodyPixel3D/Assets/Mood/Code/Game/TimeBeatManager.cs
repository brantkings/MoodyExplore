using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBeatManager : CreateableSingleton<TimeBeatManager>
{
    [System.Serializable]
    public struct BeatQuantity
    {
        [SerializeField]
        private int beats;
        public float GetTime()
        {
            return TimeBeatManager.GetTime(beats);
        }
    }

    public float beatLength = 0.125f;

    public static float GetTime(float beats)
    {
        return Instance.beatLength * beats;
    }

    public static float GetTime(int beats)
    {
        return Instance.beatLength * beats;
    }
}
