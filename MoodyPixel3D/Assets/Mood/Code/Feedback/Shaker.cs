using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Shaker : MonoBehaviour
{
    public ShakeTweenData normalData;

    public void DoShake()
    {
        Shake();
    }

    public Tween Shake()
    {
        return Shake(normalData);
    }

    public Tween Shake(ShakeTweenData data)
    {
        return data.ShakeTween(transform);
    }
}
