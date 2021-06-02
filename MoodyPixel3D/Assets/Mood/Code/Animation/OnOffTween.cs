using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OnOffTween : LHH.Switchable.SwitchableEffect
{
    public Transform toMove;

    private Transform ToMove
    {
        get
        {
            if (toMove == null) return transform;
            else return toMove;
        }
    }

    public Vector3 onPositionRelative;
    public Vector3 onRotationRelative;
    public Transform onPositionOverride;

    private Vector3 onLocalPosition;
    private Vector3 offLocalPosition;
    private Vector3 onLocalRotation;
    private Vector3 offLocalRotation;

    public float durationIn = 1f;
    public float durationOut = 1f;
    public Ease easeIn = Ease.Linear;
    public Ease easeOut = Ease.Linear;

    protected override void Awake()
    {
        base.Awake();

        onLocalPosition = onPositionRelative;
        offLocalPosition = ToMove.localPosition;
        onLocalRotation = onRotationRelative;
        offLocalRotation = ToMove.localRotation.eulerAngles;

        if(onPositionOverride != null)
        {
            onLocalPosition = onPositionOverride.position - ToMove.position;
            onLocalRotation = onPositionOverride.rotation.eulerAngles - ToMove.rotation.eulerAngles;
        }
    }

    protected override void Effect(bool on)
    {
        Debug.LogFormat("{0} is effecting to {1}", this, on);
        if(on)
        {
            TweenTo(onLocalPosition, onLocalRotation, durationIn, easeIn, easeIn);
        }
        else
        {
            TweenTo(offLocalPosition, offLocalRotation, durationOut, easeOut, easeOut);
        }
    }

    private void TweenTo(Vector3 localPosition, Vector3 localRotation, float duration, Ease easePos, Ease easeRot)
    {
        ToMove.DOLocalMove(localPosition, duration).SetEase(easePos);
        ToMove.DOLocalRotate(localRotation, duration).SetEase(easeRot);
    }
}
