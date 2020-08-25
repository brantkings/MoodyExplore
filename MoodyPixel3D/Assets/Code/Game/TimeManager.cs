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

    private float _currentTargetDeltaTime = 1f;
    public float deltaTimeSlowDuration;
    public Ease deltaTimeSlowEase = Ease.OutSine;
    public float deltaTimeReturnToNormalDuration;
    public Ease deltaTimeReturnEase = Ease.InSine;
    
    
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
            _currentTween = TweenTime(GetTargetDeltaTime(), data.tweenDuration).SetEase(data.ease);
            yield return _currentTween;
        }
        else
        {
            Time.timeScale = GetTargetDeltaTime();
        }
    }

    private float GetTargetDeltaTime()
    {
        return _currentTargetDeltaTime;
    }

    private Tween TweenTime(float to, float duration)
    {
        return DOTween.To(() => Time.timeScale, (x) => Time.timeScale = x, to, duration).SetUpdate(true);
    }

    public void ReturnTimeToNormal()
    {
        ChangeTimeDelta(1f, deltaTimeReturnToNormalDuration, deltaTimeReturnEase);
    }

    public void ChangeTimeDelta(float targetTimeDelta)
    {
        ChangeTimeDelta(targetTimeDelta, deltaTimeSlowDuration, deltaTimeSlowEase);
    }
    
    
    public void ChangeTimeDelta(float targetTimeDelta, float duration, Ease ease = Ease.OutSine)
    {
        _currentTargetDeltaTime = targetTimeDelta;
        TweenTime(_currentTargetDeltaTime, duration).SetEase(ease);
    }
}
