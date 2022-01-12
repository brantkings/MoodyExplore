using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{
    [RequireComponent(typeof(Camera))]
    public class SameCameraData : MonoBehaviour
    {
        Camera cam;
        public Camera mainCamera;

        public bool sameFieldOfView;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }
        // Update is called once per frame
        void LateUpdate()
        {
            if (sameFieldOfView) cam.fieldOfView = mainCamera.fieldOfView;
        }
    }

}
