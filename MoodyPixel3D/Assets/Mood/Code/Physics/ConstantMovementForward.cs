using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantMovementForward : MonoBehaviour
{

    public float velocity;
    
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * velocity * Time.deltaTime;
    }
}
