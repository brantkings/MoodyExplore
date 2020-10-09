using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    private Camera _main;
    
    void Start()
    {
        _main = Camera.main;
    }

    void LateUpdate()
    {
        transform.forward = _main.transform.forward;
    }
}
