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
        public SkillDirectionSanitizer directionFixer;
        public bool warningOnHit;
        public float effectDistance;
    }
    
    [SerializeField]
    private SpriteRenderer _rend;

    [SerializeField]
    private SpriteRenderer _rendEffect;

    [SerializeField]
    private Transform _resultPositionMark;
    private SpriteRenderer _resultRend;
    
    private SpriteRenderer RendererNormal
    {
        get
        {
            if(_rend == null) _rend = GetComponentInChildren<SpriteRenderer>();
            return _rend;
        }
    }

    private SpriteRenderer RendererEffect
    {
        get
        {
            return _rendEffect;
        }
    }

    public float durationIn = 1f;
    public Ease easeIn;
    public float durationOut = 2f;
    public Ease easeOut;
    public float expandOutFactor = 1.1f;
    public float durationRotateFactor = 0.025f;
    public float resultPositionAlphaValue = 1f;
    public Vector3 offsetPosition = Vector3.up * .15f;

    public Gradient colorIn;
    public Gradient colorOut;
    public Color colorFine = Color.green;
    public Color colorEffect = Color.blue;
    public Color colorWarning = Color.red;

    private Gradient gradientNow;
    private Color colorNow;

    private Tween _tweenNow;
    private float _targetWidth = 0f;

    private Properties _parametersInEffect;

    private MoodPawn pawn;

    private void Awake()
    {
        pawn = GetComponentInParent<MoodPawn>();
        _resultRend = _resultPositionMark.GetComponentInChildren<SpriteRenderer>();
    }


    public override void Show(MoodPawn pawn, Properties arrowParameters)
    {
        
        _tweenNow.KillIfActive();
        Sequence seq = DOTween.Sequence();

        colorNow = colorFine;

        RendererNormal.size = Vector2.zero;
        //seq.Insert(0f, TweenDirection(direction, durationIn * durationRotateFactor));
        seq.Insert(0f, TweenSpriteWidth(arrowParameters.width, durationIn, easeIn));
        seq.Insert(0f, TweenColor(colorIn, durationIn));
        seq.Insert(0f, RendererNormal.DOFade(0.5f, durationIn).SetEase(easeIn));
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
        seq.Insert(0f, RendererNormal.DOFade(0f, durationOut).SetEase(easeOut));
        seq.SetUpdate(true);

        _resultPositionMark.gameObject.SetActive(false);

        _tweenNow = seq;
        _parametersInEffect = null;
    }

    private float GetArrowLength(Vector3 direction, out bool hitted)
    {
        hitted = false;
        if (_parametersInEffect != null)
        {
            float desiredLength = Mathf.Clamp(direction.magnitude, _parametersInEffect.directionFixer.minLength, _parametersInEffect.directionFixer.maxLength);
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

    
    
    public void SetDirection(MoodPawn pawn, MoodSkill skill, Vector3 skillDirection)
    {
        //Renderer.transform.position = pawn.GetSkillPreviewOriginPosition() + offsetPosition;
        RendererNormal.transform.rotation = Quaternion.LookRotation(Vector3.up, skillDirection.normalized);
        float length = GetArrowLength(skillDirection, out bool hitted);
        _resultPositionMark.gameObject.SetActive(length > 0f);
        RendererNormal.size = new Vector2(_targetWidth, length);
        _resultPositionMark.localPosition = transform.InverseTransformDirection(skillDirection.normalized * length);
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
        Color oldColor = RendererNormal.color;
        Color gradientColor = gradientNow.Evaluate(x);
        Color resultColor = Color.Lerp(gradientColor, colorNow, x);
        resultColor.a = oldColor.a;
        RendererNormal.color = resultColor;
        resultColor.a = resultPositionAlphaValue;
        _resultRend.color = resultColor;
    }

    private float GetColor()
    {
        return _colorLerpNum;
    }
    
    private Tween TweenDirection(Vector3 to, float duration)
    {
        return RendererNormal.transform.DOLookAt(to, duration, AxisConstraint.None, Vector3.up);
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
