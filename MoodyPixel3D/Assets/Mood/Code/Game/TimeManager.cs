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
        public int delayFrames;
        public float delayDuration;
        public int freezeFrames;
        public float freezeDuration;
        public float tweenDuration;
        public float minTimeScale;
        public Ease ease;

        public override string ToString()
        {
            return string.Format("[Framefreeze:{0} deltaTime, {1} frs + {2} rtscnds, delay {3} frs + {4} rtscnds, tween {5} secs in {6}]", minTimeScale, freezeFrames, freezeDuration, 
                delayFrames, delayDuration, tweenDuration, ease);
        }

        /// <summary>
        /// This might be wrong because includes summing real time with not real time values. If the timescale changes, this will be an error for sure..
        /// </summary>
        /// <param name="plusDelay"></param>
        /// <returns></returns>
        public float GetTotalDuration(bool plusDelay = true)
        {
            float duration = 0f;
            if (plusDelay) duration += delayFrames * 1f / Application.targetFrameRate + delayDuration;
            duration += freezeFrames * 1f / Application.targetFrameRate + freezeDuration;
            duration += tweenDuration;
            return duration;
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
        float timeScaleNow = GetTimeScaleNow();
        Time.timeScale = timeScaleNow;
        if (fmodTimescaleParameter != null) fmodTimescaleParameter.SetParameter(timeScaleNow);
    }

    private float GetTimeScaleNow()
    {
        float dynamicTimeScale;
        float minDynamicTimeScale;
        float maxDynamicTimeScale;

        dynamicTimeScale = GetCurrentDynamicTimeDeltaTarget(out minDynamicTimeScale, out maxDynamicTimeScale);

        return Mathf.Clamp(dynamicTimeScale * _staticTimeScale, Mathf.Min(_minStaticTimeScale, minDynamicTimeScale), Mathf.Max(_maxStaticTimeScale, maxDynamicTimeScale));
    }

    private void OnDisable() 
    {
        if(fmodTimescaleParameter != null) fmodTimescaleParameter.ResetParameterToNeutralValue();
    }

    Coroutine freezeFrameRoutine = null;

    public void FreezeFrames(in FrameFreezeData data)
    {
        if (freezeFrameRoutine != null) StopCoroutine(freezeFrameRoutine);
        freezeFrameRoutine = StartCoroutine(StopTimeRoutine(data));
    }

    private IEnumerator StopTimeRoutine(FrameFreezeData data)
    {
        float time = Time.unscaledTime;
        Debug.LogWarningFormat("[TIMEMANAGER] Freezing frame with {0} ({1}, {2})", data, Time.unscaledTime, Time.unscaledTime - time);
        for (int i = 0; i < data.delayFrames; i++) yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(data.delayDuration);
        _currentTween.CompleteIfActive();
        _staticTimeScale = data.minTimeScale;
        _minStaticTimeScale = data.minTimeScale;
        _maxStaticTimeScale = data.minTimeScale;
        Debug.LogFormat("[TIMEMANAGER] Time scale is now {0} -> {1} ({2}, {3})", _staticTimeScale, GetTimeScaleNow(), Time.unscaledTime, Time.unscaledTime - time);
        if (data.freezeFrames > 0) for (int i = 0; i < data.freezeFrames; i++) yield return new WaitForEndOfFrame();
        if (data.freezeDuration > 0f) yield return new WaitForSecondsRealtime(data.freezeDuration);
        Debug.LogFormat("[TIMEMANAGER] Waited with time scale as {0} -> {1} ({2}, {3})", _staticTimeScale, GetTimeScaleNow(), Time.unscaledTime, Time.unscaledTime - time);
        if (data.tweenDuration > 0f)
        {
            _currentTween = TweenTime(GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale), data.tweenDuration).SetEase(data.ease);
            yield return _currentTween;
        }
        else
        {
            _staticTimeScale = GetCurrentStaticTimeDeltaTarget(out _minStaticTimeScale, out _maxStaticTimeScale);
        }
        Debug.LogFormat("[TIMEMANAGER] Finally, time scale is now {0} -> {1} ({2}, {3})", _staticTimeScale, GetTimeScaleNow(), Time.unscaledTime, Time.unscaledTime - time);
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
        float targetProduct = 1f;

        if (targetDynamicTimeScale != null)
        {
            foreach (KeyValuePair<string, ITimeDeltaGetter> v in targetDynamicTimeScale)
            {
                if (v.Value.Equals(null)) continue;
                targetMin = Mathf.Min(v.Value.GetTimeDeltaNow(), targetMin);
                targetMax = Mathf.Max(v.Value.GetTimeDeltaNow(), targetMax);
                targetProduct = targetProduct * v.Value.GetTimeDeltaNow();
            }
            return Mathf.Clamp(targetProduct, targetMin, targetMax);
        }
        else return 1f;

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
        Debug.LogFormat("[TIME] Time delta going to {0} from {1} in {2} seconds and {3} easing.", _staticTimeScale, targetTimeDelta, duration, ease);
        TweenTime(targetTimeDelta, duration).SetEase(ease);
    }

    private Tween TweenTime(float to, float duration)
    {
        return DOTween.To(() => _staticTimeScale, (x) => _staticTimeScale = x, to, duration).SetUpdate(true);
    }
}
