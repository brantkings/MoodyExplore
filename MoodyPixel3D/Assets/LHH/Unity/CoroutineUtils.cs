using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity.Coroutine
{
    public static class CoroutineUtils
    {
        public static IEnumerator TimedRoutine(System.Action func, float time)
        {
            yield return new WaitForSeconds(time);
            func();
        }

        public static IEnumerator TimedRealtimeRoutine(System.Action func, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            func();
        }
    }
}
