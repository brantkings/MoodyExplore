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

    public interface ITimeDeltaGetter
    {
        float GetTimeDeltaNow();
    }

    private Dictionary<string, float> targetTimeScale;
    private Dictionary<string, ITimeDeltaGetter> targetDynamicTimeScale;

    private float _minStaticTimeScale;
    private float _maxStaticTimeScale;
    private float _staticTimeScale;


    private Tween _currentTween;

    public GlobalSoundParameter fmodTimescaleParameter;

    private void Start()
    {
        Time.timeScale = _staticTimeScale = 1f;
    }

    private void Update() 
    {
        if(fmodTimescaleParameter != null) fmodTimescaleParameter.SetParameter(Time.timeScale);

        float dynamicTimeScale;
        float minDynamicTimeScale;
        float maxDynamicTimeScale;

        dynamicTimeScale = GetCurrentDynamicTimeDeltaTarget(out minDynamicTimeScale, out maxDynamicTimeScale);

        Time.timeScale = Mathf.Clamp(dynamicTimeScale * _staticTimeScale, Mathf.Min(_minStaticTimeScale, minDynamicTimeScale), Mathf.Max(_maxStaticTimeScale, maxDynamicTimeScale));
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
        _staticTimeScale = data.minTimeScale;
        if (data.freezeFrames > 0) for (int i = 0; i < data.freezeFrames; i++) yield return new WaitForEndOfFrame();
        if (data.freezeDuration > 0f) yield return new WaitForSecondsRealtime(data.freezeDuration);
        if (data.tweenDuration > 0f)
        {
            _currentTween = TweenTime(GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale), data.tweenDuration).SetEase(data.ease);
            yield return _currentTween;
        }
        else
        {
            _staticTimeScale = GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale);
        }
    }


    private void AddTimeDeltaTarget(string id, float target)
    {
        if (targetTimeScale == null)
        {
            targetTimeScale = new Dictionary<string, float>(8);
        }
        targetTimeScale.Add(id, target);
    }

    public void AddDynamicDeltaTarget(string id, ITimeDeltaGetter func)
    {
        if (targetDynamicTimeScale == null)
        {
            targetDynamicTimeScale = new Dictionary<string, ITimeDeltaGetter>(8);
        }
        targetDynamicTimeScale.Add(id, func);
    }

    private void RemoveTimeDeltaTarget(string id)
    {
        targetTimeScale?.Remove(id);
    }

    public void RemoveDynamicTimeDeltaTarget(string id)
    {
        targetDynamicTimeScale?.Remove(id);
    }

    private float GetCurrentDynamicTimeDeltaTarget(out float targetMin, out float targetMax)
    {
        targetMin = 1f;
        targetMax = 1f;
        if (targetTimeScale == null) return 1f;
        float targetProduct = 1f;

        if(targetDynamicTimeScale != null)
        {
            foreach (ITimeDeltaGetter func in targetDynamicTimeScale.Values)
            {
                if (func.Equals(null)) continue;
                targetMin = Mathf.Min(func.GetTimeDeltaNow(), targetMin);
                targetMax = Mathf.Min(func.GetTimeDeltaNow(), targetMax);
                targetProduct = targetProduct * func.GetTimeDeltaNow();
            }
        }

        return Mathf.Clamp(targetProduct, targetMin, targetMax);
    }

    private float GetCurrentStaticTimeDeltaTarget(out float targetMin, out float targetMax)
    {
        targetMin = 1f;
        targetMax = 1f;
        if (targetTimeScale == null) return 1f;
        float targetProduct = 1f;

        foreach (float value in targetTimeScale.Values)
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
        TweenTimeDelta(GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale), deltaTimeReturnToNormalDuration, deltaTimeReturnEase);
    }
    
    public void ChangeTimeDelta(float targetTimeDelta, string id)
    {    
        AddTimeDeltaTarget(id, targetTimeDelta);
        TweenTimeDelta(GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale), deltaTimeSlowDuration, deltaTimeChangeEase);
    }
    
    
    public void TweenTimeDelta(float targetTimeDelta, float duration, Ease ease = Ease.OutSine)
    {
        TweenTime(targetTimeDelta, duration).SetEase(ease);
    }

    private Tween TweenTime(float to, float duration)
    {
        return DOTween.To(() => _staticTimeScale, (x) => _staticTimeScale = x, to, duration).SetUpdate(true);
    }
}
