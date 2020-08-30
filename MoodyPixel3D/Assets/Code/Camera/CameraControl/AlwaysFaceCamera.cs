using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
