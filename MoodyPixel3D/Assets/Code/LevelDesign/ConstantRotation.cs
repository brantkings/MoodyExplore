using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 eulerVelocity;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(eulerVelocity * Time.deltaTime) * transform.rotation;
    }
}
