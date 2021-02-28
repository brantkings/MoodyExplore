using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RangeArrow : RangeShow<RangeArrow.Properties>, IRangeShowDirected
{
    [System.Serializable]
    public class Properties
    {
        public float width = 1f;
        public float minLength;
        public float maxLength;
        public float direction;
        public bool warningOnHit;
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
    public Color colorFine;
    public Color colorWarning;

    private Gradient gradientNow;
    private Color colorNow;

    private Tween _tweenNow;
    private float _targetWidth = 0f;

    private Properties _parametersInEffect;

    private MoodPawn pawn;

    private void Awake()
    {
        pawn = GetComponentInParent<MoodPawn>();
    }


    public override void Show(MoodPawn pawn, Properties arrowParameters)
    {
        
        _tweenNow.KillIfActive();
        Sequence seq = DOTween.Sequence();

        colorNow = colorFine;


        Renderer.size = Vector2.zero;
        //seq.Insert(0f, TweenDirection(direction, durationIn * durationRotateFactor));
        seq.Insert(0f, TweenSpriteWidth(arrowParameters.width, durationIn, easeIn));
        seq.Insert(0f, TweenColor(colorIn, durationIn));
        seq.Insert(0f, Renderer.DOFade(0.5f, durationIn).SetEase(easeIn));
        seq.SetUpdate(true);
        
        _tweenNow = seq;
        _parametersInEffect = arrowParameters;
    }

    public override void Hide(MoodPawn pawn)
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

    private float GetArrowLength(Vector3 direction, out bool hitted)
    {
        hitted = false;
        if (_parametersInEffect != null)
        {
            float desiredLength = Mathf.Clamp(direction.magnitude, _parametersInEffect.minLength, _parametersInEffect.maxLength);
            LHH.Caster.Caster caster = pawn.mover.GetCaster(KinematicPlatformer.CasterClass.Side);
            RaycastHit hit;
            if (Physics.Raycast(caster.transform.position, direction.normalized, out hit, desiredLength, caster.HitMask | caster.ObstacleMask))
            {
                //Debug.LogFormat("What is there is {0} ({1} to {2})", hit.collider, desiredLength, hit.distance);
                desiredLength = hit.distance;
                hitted = true;
                return desiredLength;
            }
            /*if (caster.CastLength(direction.normalized * desiredLength, out hit))
            {
                Debug.LogFormat("What is there is {0} ({1} to {2})", hit.collider, desiredLength, hit.distance);
                desiredLength = hit.distance + caster.SafetyDistance;
            }*/
            return desiredLength;
            
        }
        else return 0f;
    }

    private bool NeedsWarning()
    {
        if (_parametersInEffect != null) return _parametersInEffect.warningOnHit;
        else return false;
    }

    
    
    public void SetDirection(Vector3 skillDirection)
    { 
        Renderer.transform.rotation = Quaternion.LookRotation(Vector3.up, skillDirection.normalized);
        Renderer.size = new Vector2(_targetWidth, GetArrowLength(skillDirection, out bool hitted));
        if(NeedsWarning())
        {
            colorNow = hitted ? colorWarning : colorFine;
            SetColor(_colorLerpNum);
        }
        
    }
    
    private Tween TweenColor(Gradient to, float duration)
    {
        gradientNow = to;
        return DOTween.To(GetColor, SetColor, 1f, duration).SetEase(Ease.Linear);
    }

    private float _colorLerpNum;

    private void SetColor(float x)
    {
        _colorLerpNum = x;
        Color oldColor = Renderer.color;
        Color gradientColor = gradientNow.Evaluate(x);
        Color resultColor = Color.Lerp(gradientColor, colorNow, x);
        resultColor.a = oldColor.a;
        Renderer.color = resultColor; 
    }

    private float GetColor()
    {
        return _colorLerpNum;
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
