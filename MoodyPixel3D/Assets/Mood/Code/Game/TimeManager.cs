using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

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

    public float deltaTimeSlowDuration;
    [FormerlySerializedAs("deltaTimeSlowEase")] public Ease deltaTimeChangeEase = Ease.OutSine;
    public float deltaTimeReturnToNormalDuration;
    public Ease deltaTimeReturnEase = Ease.InSine;

    private Dictionary<string, float> targetDeltaTime;
    
    
    private Tween _currentTween;

    public GlobalSoundParameter fmodTimescaleParameter;

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update() 
    {
        if(fmodTimescaleParameter != null) fmodTimescaleParameter.SetParameter(Time.timeScale);
    }

    private void OnDisable() 
    {
        if(fmodTimescaleParameter != null) fmodTimescaleParameter.ResetParameterToNeutralValue();
    }

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
            _currentTween = TweenTime(GetCurrentTimeDeltaTarget(), data.tweenDuration).SetEase(data.ease);
            yield return _currentTween;
        }
        else
        {
            Time.timeScale = GetCurrentTimeDeltaTarget();
        }
    }

    private Tween TweenTime(float to, float duration)
    {
        return DOTween.To(() => Time.timeScale, (x) => Time.timeScale = x, to, duration).SetUpdate(true);
    }

    private void AddTimeDeltaTarget(string id, float target)
    {
        if (targetDeltaTime == null)
        {
            targetDeltaTime = new Dictionary<string, float>(8);
        }
        targetDeltaTime.Add(id, target);
    }

    private void RemoveTimeDeltaTarget(string id)
    {
        targetDeltaTime?.Remove(id);
    }

    private float GetCurrentTimeDeltaTarget()
    {
        if (targetDeltaTime == null) return 1f;
        float targetMin = 1f;
        float targetMax = 1f;
        float targetProduct = 1f;
        foreach (float value in targetDeltaTime.Values)
        {
            targetMin = Mathf.Min(value, targetMin);
            targetMax = Mathf.Min(value, targetMax);
            targetProduct = targetProduct * value;
        }

        return Mathf.Clamp(targetProduct, targetMin, targetMax);
    }
    
    public void RemoveTimeDeltaChange(string id)
    {
        RemoveTimeDeltaTarget(id);
        TweenTimeDelta(GetCurrentTimeDeltaTarget(), deltaTimeReturnToNormalDuration, deltaTimeReturnEase);
    }
    
    public void ChangeTimeDelta(float targetTimeDelta, string id)
    {    
        AddTimeDeltaTarget(id, targetTimeDelta);
        TweenTimeDelta(GetCurrentTimeDeltaTarget(), deltaTimeSlowDuration, deltaTimeChangeEase);
    }
    
    
    public void TweenTimeDelta(float targetTimeDelta, float duration, Ease ease = Ease.OutSine)
    {
        TweenTime(targetTimeDelta, duration).SetEase(ease);
    }
}
