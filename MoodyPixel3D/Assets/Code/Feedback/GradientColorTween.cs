using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GradientColorTween<T> : TweenOnEnable<T, float>
{
    public Gradient disabledToEnabled;
    public bool useDifferentGradientForDisable;
    public Gradient enabledToDisabled;

    public abstract void SetValue(Color value);

    private Gradient _toUse;

    protected override Tween ExecuteTweenItself(float to, float duration)
    {
        return DOTween.To(Get, Set, to, duration);
    }

    private float _currentValue;
    private float Get()
    {
        return _currentValue;
    }
    private void Set(float v)
    {
        _currentValue = v;
        SetValue(disabledToEnabled.Evaluate(v));
    }

    private void SelectGradient()
    {

    }

    public override void SetValue(float value)
    { 
        Set(value);
    }

    protected override float GetInValue()
    {
        return 1f;
    }

    protected override float GetOutValue()
    {
        return 0f;
    }
}
