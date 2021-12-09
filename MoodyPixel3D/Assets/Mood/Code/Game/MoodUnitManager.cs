using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodUnitManager : CreateableSingleton<MoodUnitManager>
{
    public abstract class UnitBeats
    {
        [SerializeField] internal float beats;

        protected abstract float GetBeatLength();
        public float GetLength()
        {
            return GetLength(beats);
        }

        protected float GetLength(float beatValue)
        {
            return beatValue * GetBeatLength();
        }

        /// <summary>
        /// For those times when you need the amount to get to 1 unit.
        /// </summary>
        /// <returns></returns>
        public float GetInversedLength()
        {
            if (beats != 0f) return 1f / GetLength();
            else return 0f;
        }


        public void SetLength(float length)
        {
            beats = GetBeatsFromLength(length, GetBeatLength());
        }

        protected static float GetBeatsFromLength(float lengthWanted, float beatLength)
        {
            if (beatLength != 0f)
            {
                return lengthWanted / beatLength;
            }
            else
            {
                return 0f;
            }
        }


    }

    [System.Serializable]
    public class TimeBeats : UnitBeats
    {
        protected override float GetBeatLength()
        {
            return MoodUnitManager.GetTimeBeatLength();
        }

        public static implicit operator TimeBeats(int q)
        {
            return new TimeBeats()
            {
                beats = Mathf.FloorToInt(q)
            };
        }

        public static implicit operator TimeBeats(float q)
        {
            var t = new TimeBeats();
            t.SetLength(q);
            return t;
        }

        public static implicit operator float(TimeBeats q)
        {
            return q.GetLength();
        }
    }

    [System.Serializable]
    public class DistanceBeats : UnitBeats
    {
        protected override float GetBeatLength()
        {
            return MoodUnitManager.GetDistanceBeatLength();
        }

        public static implicit operator DistanceBeats(int q)
        {
            return new DistanceBeats()
            {
                beats = Mathf.FloorToInt(q)
            };
        }

        public static implicit operator DistanceBeats(float q)
        {
            var t = new DistanceBeats();
            t.SetLength(q);
            return t;
        }

        public static implicit operator float(DistanceBeats q)
        {
            return q.GetLength();
        }
    }

    [System.Serializable]
    public class SpeedBeats : UnitBeats
    {
        protected override float GetBeatLength()
        {
            return MoodUnitManager.GetDistanceBeatLength() / MoodUnitManager.GetTimeBeatLength();
        }

        public static implicit operator SpeedBeats(int q)
        {
            return new SpeedBeats()
            {
                beats = Mathf.FloorToInt(q)
            };
        }

        public static implicit operator SpeedBeats(float q)
        {
            var t = new SpeedBeats();
            t.SetLength(q);
            return t;
        }

        public static implicit operator float(SpeedBeats q)
        {
            return q.GetLength();
        }
    }

    [UnityEngine.Serialization.FormerlySerializedAs("beatLength")]
    public float beatTimeLength = 0.125f;

    public float beatDistanceLength = 0.25f;

    public static float GetTime(float beats)
    {
        return Instance.beatTimeLength * beats;
    }

    public static float GetTime(int beats)
    {
        return Instance.beatTimeLength * beats;
    }

    public static float GetNumberOfBeats(float length)
    {
        return length / Instance.beatTimeLength;
    }

    public static float GetTimeBeatLength()
    {
        return Instance.beatTimeLength;
    }

    public static float GetDistanceBeatLength()
    {
        return Instance.beatDistanceLength;
    }

}
