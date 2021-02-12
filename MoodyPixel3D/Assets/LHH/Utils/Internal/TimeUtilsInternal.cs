using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils.Internal
{
    [System.Serializable]
    public abstract class TimeStamp<T>
    {
        T stamp;
        [SerializeField]
        T timeAvailable;

        public void StartTimer()
        {
            stamp = GetNowValue();
        }

        public void StartTimer(T newTime)
        {
            timeAvailable = newTime;
            StartTimer();
        }

        public bool IsInTime()
        {
            int c = Comparer<T>.Default.Compare(Subtract(GetNowValue(), stamp), timeAvailable);

            return c <= 0;
        }

        public abstract T Subtract(T x, T y);

        protected abstract T GetNowValue();
    }
}
