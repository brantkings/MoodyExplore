using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ai_K_UniformMovementToTarget : MonoBehaviour
{
    public Detector detector;
    public UniformMovement movement;

    [Space()]
    public float velocityDetecting = 1f;
    public bool canUseX = true;
    public bool canUseY = false;
    public bool canUseZ = true;



    private void OnEnable()
    {
        if(detector != null)
        {
            detector.OnDetect += OnDetect;
        }
    }

    private void OnDetect()
    {
        Debug.LogFormat("{0} is detecting", this);
        Vector3? dir = detector?.GetDistanceToTarget();
        if(dir.HasValue) movement.SetDirection(ParseDirection(dir.Value));
    }

    private Vector3 ParseDirection(Vector3 vec)
    {
        if (!canUseX) vec.x = 0f;
        if (!canUseY) vec.y = 0f;
        if (!canUseZ) vec.z = 0f;
        vec = vec.normalized * velocityDetecting;
        return vec;
    }
}
