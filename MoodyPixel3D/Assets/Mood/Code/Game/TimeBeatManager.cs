using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBeatManager : CreateableSingleton<TimeBeatManager>
{
    [System.Serializable]
    public struct BeatQuantity
    {
        [SerializeField]
        private float beats;
        public float GetTime()
        {
            return TimeBeatManager.GetTime(beats);
        }

        public float GetInversedTime()
        {
            if (beats != 0f) return 1f / GetTime();
            else return 0f;
        }

        public BeatQuantity SetTime(float time)
        {
            beats = TimeBeatManager.GetNumberOfBeats(time);
            return this;
        }

        public static implicit operator BeatQuantity(int q)
        {
            return new BeatQuantity()
            {
                beats = Mathf.FloorToInt(q)
            };
        }

        public static implicit operator BeatQuantity(float q)
        {
            return new BeatQuantity().SetTime(q);
        }

        public static implicit operator float(BeatQuantity q)
        {
            return q.GetTime();
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

    public static float GetNumberOfBeats(float length)
    {
        return length / Instance.beatLength;
    }

    public static float GetBeatLength()
    {
        return Instance.beatLength;
    }

}
