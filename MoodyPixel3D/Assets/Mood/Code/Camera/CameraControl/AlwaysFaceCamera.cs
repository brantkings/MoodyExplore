using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    private Camera _main;
    
    void Start()
    {
        _main = Camera.main;
        if (_main == null) enabled = false;
    }

    void LateUpdate()
    {
        transform.forward = _main.transform.forward;
    }
}
