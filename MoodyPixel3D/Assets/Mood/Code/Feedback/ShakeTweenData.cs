using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


[CreateAssetMenu(menuName = "Long Hat House/Events/Shake", fileName = "Shake_")]
public class ShakeTweenData : ScriptableEvent<Tween>
{
    [Header("Shake")]
    public Vector3 force;
    public int vibrato = 10;
    public float randomness = 90f;
    public bool snapping = false;
    public bool fadeOut = true;
    public Ease ease = Ease.Linear;
    public bool isIndependentUpdate = false;

    public float duration;

    public override Tween InvokeReturn(Transform where)
    {
        return ShakeTween(where); 
    }

    public Tween ShakeTween(Transform t)
    {
        if (t == null) return null;
        return t.DOShakePosition(duration, force, vibrato, randomness, snapping, fadeOut).SetUpdate(isIndependentUpdate).SetEase(ease);
    }
}
