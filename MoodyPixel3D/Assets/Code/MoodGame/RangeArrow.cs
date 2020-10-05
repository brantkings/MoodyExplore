using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RangeArrow : RangeShow<RangeArrow.Properties>
{
    [System.Serializable]
    public class Properties
    {
        public float width = 1f;
        public float minLength;
        public float maxLength;
    }
    
    private SpriteRenderer _rend;
    
    private SpriteRenderer Renderer
    {
        get
        {
            if(_rend == null) _rend = GetComponentInChildren<SpriteRenderer>();
            return _rend;
        }
    }

    public float durationIn = 1f;
    public Ease easeIn;
    public float durationOut = 2f;
    public Ease easeOut;
    public float expandOutFactor = 1.1f;
    public float durationRotateFactor = 0.025f;

    public Gradient colorIn;
    public Gradient colorOut;

    private Tween _tweenNow;
    private float _targetWidth = 0f;

    private Properties _parametersInEffect;


    public override void Show(Properties arrowParameters)
    {
        
        _tweenNow.KillIfActive();
        Sequence seq = DOTween.Sequence();

        
        Renderer.size = Vector2.zero;
        //seq.Insert(0f, TweenDirection(direction, durationIn * durationRotateFactor));
        seq.Insert(0f, TweenSpriteWidth(arrowParameters.width, durationIn, easeIn));
        seq.Insert(0f, TweenColor(colorIn, durationIn));
        seq.Insert(0f, Renderer.DOFade(0.5f, durationIn).SetEase(easeIn));
        seq.SetUpdate(true);
        
        _tweenNow = seq;
        _parametersInEffect = arrowParameters;
    }

    public override void Hide()
    {
        _tweenNow.KillIfActive();
        Sequence seq = DOTween.Sequence();

        seq.Insert(0f, TweenSpriteWidth(GetSpriteWidth() * expandOutFactor, durationOut, easeOut));
        seq.Insert(0f, TweenColor(colorOut, durationOut));
        seq.Insert(0f, Renderer.DOFade(0f, durationOut).SetEase(easeOut));
        seq.SetUpdate(true);
        
        _tweenNow = seq;
        _parametersInEffect = null;
    }

    private float GetArrowLength(float originalMagnitude)
    {
        if (_parametersInEffect != null)
        {
            return Mathf.Clamp(originalMagnitude, _parametersInEffect.minLength, _parametersInEffect.maxLength);
        }
        else return 0f;
    }


    
    
    public void SetDirection(Vector3 direction)
    {
        Renderer.transform.rotation = Quaternion.LookRotation(Vector3.up, direction.normalized);
        Renderer.size = new Vector2(_targetWidth, GetArrowLength(direction.magnitude));
    }
    
    private Tween TweenColor(Gradient to, float duration)
    {
        return Renderer.DOGradientColor(to, duration).SetEase(Ease.Linear);
    }
    
    private Tween TweenDirection(Vector3 to, float duration)
    {
        return Renderer.transform.DOLookAt(to, duration, AxisConstraint.None, Vector3.up);
    }

    private Tween TweenSpriteWidth(float width, float duration, Ease ease)
    {
        return DOTween.To(GetSpriteWidth, SetSpriteWidth, width, duration).SetEase(ease);
    }

    private float GetSpriteWidth()
    {
        return _targetWidth;
    }

    private void SetSpriteWidth(float width)
    {
        _targetWidth = width;
    }
}
