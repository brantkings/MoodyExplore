using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantShake : MonoBehaviour
{
    public Vector3 force;

    public TransformGetter toShake;

    private Vector3 _savedLocalPos;

    private void Start()
    {
        _savedLocalPos = toShake.Get(transform).localPosition;
    }

    private void OnDisable()
    {
        toShake.Get(transform).localPosition = _savedLocalPos;
    }

    private void Update()
    {
        toShake.Get(transform).localPosition = _savedLocalPos + force.RandomRange() * 0.5f;
    }

}
