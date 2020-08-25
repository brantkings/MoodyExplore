using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RangeSphere : RangeShow<RangeSphere.Properties>
{
    [Serializable]
    public class Properties
    {
        public float radius;
    }
    
    [System.Serializable]
    private struct ConditionalFloat
    {
        [SerializeField]
        private float on;
        [SerializeField]
        private float off;

        public float GetValue(bool isOn)
        {
            return isOn ? on : off;
        }

        public ConditionalFloat(float onValue, float offValue)
        {
            on = onValue;
            off = offValue;
        }
    }
    
    private Material _material;
    [SerializeField]
    private float _duration = 1f;
    [SerializeField]
    private ConditionalFloat _colorDurationMultiplier = new ConditionalFloat(1.25f,0.4f);
    [SerializeField]
    private ConditionalFloat _velocityDurationMultiplier = new ConditionalFloat(2.5f,0.4f);
    [SerializeField]
    private Transform _toRotate;
    [SerializeField]
    private Vector3 _maxEulerRotation;
    [SerializeField]
    private Vector3 _minEulerRotation;
    [SerializeField]
    private Ease _ease = Ease.Linear;
    [SerializeField]
    private Ease _colorEase = Ease.OutQuint;
    [SerializeField]
    private float _scaleMultiplier = 1f;

    public Color showColor = new Color(0.5f,1f,0.5f,1);
    public Color hideColor = new Color(1,1,0,0);

    [SerializeField]
    [ReadOnly]
    private Vector3 _currentRotationVelocity;
    private Tween _tween;
    
    void Awake()
    {
        _material = GetComponentInChildren<Renderer>()?.material;
    }

    private void Start()
    {
        _currentRotationVelocity = _minEulerRotation;
        HideDuration(0f);
    }

    public override void Show(Properties param)
    {
        ShowDuration(param, _duration);
    }

    public void ShowDuration(Properties param, float duration)
    {
        SetRadius(0f);
        TweenRadius(duration, param.radius, true, showColor);
    }
    
    public override void Hide()
    {
        HideDuration(_duration);
    }
    
    public void HideDuration(float duration)
    {
        TweenRadius(duration, _toRotate.lossyScale.x * 1.05f, false, hideColor);
    }

    private void Update()
    {
        transform.Rotate(_currentRotationVelocity * Time.deltaTime);
    }

    private Tween TweenRadius(float duration, float radius, bool appearing, Color color)
    {
        _tween.KillIfActive();
        Sequence seq = DOTween.Sequence();
        _currentRotationVelocity = _maxEulerRotation;
        seq.Insert(0f, _material.DOColor(color, "_WireColor", duration * _colorDurationMultiplier.GetValue(appearing)).SetEase(_colorEase));
        seq.Insert(0f, DOTween.To(GetRadius, SetRadius, radius, duration).SetEase(_ease));
        seq.Insert(0f, DOTween.To(() => _currentRotationVelocity, (x) => _currentRotationVelocity = x, _minEulerRotation, duration * _velocityDurationMultiplier.GetValue(appearing)).SetEase(_ease));
        seq.SetUpdate(true);
        _tween = seq;
        return seq;
    }

    private void SetRadius(float r)
    {
        _toRotate.localScale = Vector3.one * (r * _scaleMultiplier);
    }

    private float GetRadius()
    {
        return _toRotate.localScale.x;
    }
}
