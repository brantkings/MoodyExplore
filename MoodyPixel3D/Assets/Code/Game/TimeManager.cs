using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TimeManager : CreateableSingleton<TimeManager>
{
    [System.Serializable]
    public struct FrameFreezeData
    {
        public int freezeFrames;
        public float freezeDuration;
        public float tweenDuration;
        public float minTimeScale;
        public Ease ease;

        public override string ToString()
        {
            return string.Format("[Framefreeze:{0} secs from {1}, {2}]", tweenDuration, minTimeScale, ease);
        }
    }

    private Tween _currentTween;

    public void StopTime(FrameFreezeData data)
    {
        StartCoroutine(StopTimeRoutine(data));
    }

    private IEnumerator StopTimeRoutine(FrameFreezeData data)
    {
        _currentTween.CompleteIfActive();
        Time.timeScale = data.minTimeScale;
        if (data.freezeFrames > 0) for (int i = 0; i < data.freezeFrames; i++) yield return new WaitForEndOfFrame();
        if (data.freezeDuration > 0f) yield return new WaitForSecondsRealtime(data.freezeDuration);
        if (data.tweenDuration > 0f)
        {
            _currentTween = TweenTime(1f, data.tweenDuration).SetEase(data.ease);
            yield return _currentTween;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private Tween TweenTime(float to, float duration)
    {
        return DOTween.To(() => Time.timeScale, (x) => Time.timeScale = x, to, duration).SetUpdate(true);
    }
}
