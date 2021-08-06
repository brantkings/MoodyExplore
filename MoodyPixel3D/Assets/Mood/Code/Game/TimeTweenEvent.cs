using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LHH.ScriptableObjects.Events;

[CreateAssetMenu(menuName = "Mood/Event/Time delta tween", fileName = "E_Time_TimeDelta_", order = 0)]
public class TimeTweenEvent : ScriptableEvent
{
    [System.Serializable]
    public struct TweenData
    {
        public float from;
        public float to;
        public float duration;
        public Ease ease;
    }

    private class TweenState : TimeManager.ITimeDeltaGetter
    {
        public float timeDelta;
        public float GetTimeDeltaNow()
        {
            //Debug.LogFormat("getting time delta {0}", timeDelta);
            return timeDelta;
        }

        public void SetTimeDeltaNow(float x)
        {
            //Debug.LogFormat("setting time delta {0}", timeDelta);
            timeDelta = x;
        }
    }

    public TweenData[] tweens;

    public override void Invoke(Transform where)
    {
        TimeManager.Instance.StartCoroutine(InvokeRoutine(where));
    }

    public IEnumerator InvokeRoutine(Transform where)
    {
        TweenState state = new TweenState();
        state.SetTimeDeltaNow(1f);

        TweenCallback<float> updateFunc = (x) =>
        {
            //Debug.LogFormat("Setting {0} as {1} [{2}]", state.timeDelta, x, Time.frameCount);
            state.SetTimeDeltaNow(x);
        };

        string id = this.name + where.name;

        //Debug.LogErrorFormat("Removing as {0} to {1} BEFORE", id, where);
        TimeManager.Instance.RemoveDynamicTimeDeltaTarget(id);

        //Debug.LogErrorFormat("Adding as {0} to {1}", id, where);
        TimeManager.Instance.AddDynamicDeltaTarget(id, state);

        foreach (var t in tweens)
        {
            DOVirtual.Float(t.from, t.to, t.duration, updateFunc).SetEase(t.ease).SetUpdate(true);
            yield return new WaitForSecondsRealtime(t.duration);
        }

        TimeManager.Instance.RemoveDynamicTimeDeltaTarget(id);
        //Debug.LogErrorFormat("Removing as {0} to {1} AFTER", id, where);


    }
}
