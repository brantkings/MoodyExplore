using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RangeSphere : MonoBehaviour
{
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
        Hide(0f);
    }

    public void Show(float radius)
    {
        Show(radius, _duration);
    }

    public void Show(float radius, float duration)
    {
        SetRadius(0f);
        TweenRadius(duration, radius, true, showColor);
    }
    
    public void Hide()
    {
        Hide(_duration);
    }
    
    public void Hide(float duration)
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
