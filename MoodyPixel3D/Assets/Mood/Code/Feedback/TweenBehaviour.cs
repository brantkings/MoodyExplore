using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using DG.Tweening;
using UnityEngine;

public abstract class TweenBehaviour<T,P> : LHH.Unity.AddonParentBehaviour<T>
{
    public float durationIn;
    public Ease easeIn;
    public float durationOut;
    public Ease easeOut;

    public bool unscaledTimeDelta;

    public enum Action
    {
        DisableThisAndImage,
        DeactivateObject,
        None
    }

    public Action afterFadeOut = Action.DeactivateObject;

    public bool doOnEnable = true;
    
    void OnEnable()
    {
        SetValue(GetOutValue());
        if(doOnEnable)
            TweenIn();
    }

    void OnDisable()
    {
        InterruptTween();
    }

    protected virtual void InterruptTween()
    {
        DOTween.Kill(this);
        SetValue(GetOutValue());
    }
    
    /// <summary>
    /// Implement the tweens itself. Do not implement eases or Id or anything like that, just the pure linear tween.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    protected abstract Tween ExecuteTweenItself(P to, float duration);

    protected abstract P GetInValue();
    protected abstract P GetOutValue();
    public abstract void SetValue(P value);

    private Tween _currentTween;
    
    public virtual Tween TweenTo(P to, float duration, Ease ease)
    {
        DOTween.Complete(this);
        return _currentTween = ExecuteTweenItself(to, duration).SetId(this).SetEase(ease).SetUpdate(unscaledTimeDelta);
    }

    public void DoIn()
    {
        SetValue(GetOutValue());
        TweenIn();
    }

    public void DoOut()
    {
        SetValue(GetInValue());
        TweenOut();
    }

    public virtual Tween TweenIn()
    {
        return TweenTo(GetInValue(), durationIn, easeIn);
    }
    
    public virtual Tween TweenOut()
    {
        return TweenTo(GetOutValue(), durationOut, easeOut).OnComplete(AfterFadeOut);
    }
    
    private void AfterFadeOut()
    {
        switch (afterFadeOut)
        {
            case Action.DisableThisAndImage:
                Behaviour b = Addon as Behaviour;
                if(b != null) b.enabled = false;
                enabled = false;
                break;
            case Action.DeactivateObject:
                gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }
}
