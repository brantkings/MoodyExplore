using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToCamera : MonoBehaviour {

    Camera main;

    public float smoothTime = 0f;
    private Vector3 dampVelocity;

    void Awake()
    {
        main = Camera.main;
    }

	void Update()
    {
        Vector3 targetForward = Vector3.ProjectOnPlane(main.transform.forward, Vector3.up);
        Vector3 result = Vector3.SmoothDamp(transform.forward, targetForward, ref dampVelocity, smoothTime);
        transform.forward = result;
    }
}
