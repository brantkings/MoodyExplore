using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{

    [System.Serializable]
    public class TimeStampSeconds : Internal.TimeStamp<float>
    {
        public override float Subtract(float x, float y)
        {
            return x - y;
        }

        protected override float GetNowValue()
        {
            return Time.time;
        }
    }

    [System.Serializable]
    public class TimeStampFrames : Internal.TimeStamp<int>
    {
        public override int Subtract(int x, int y)
        {
            return x - y;
        }

        protected override int GetNowValue()
        {
            return Time.frameCount;
        }
    }

    public static class TimeUtils 
    {
    }
}
