using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 eulerVelocity;
    public bool unscaledTimeDelta;

    public float GetTimeDelta()
    {
        return unscaledTimeDelta ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(eulerVelocity * GetTimeDelta()) * transform.rotation;
    }
}
