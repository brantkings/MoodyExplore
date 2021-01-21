using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraApplyMaterial : MonoBehaviour {

    public Material[] passes;
    DepthTextureMode depthTextureMode = DepthTextureMode.Depth;

    Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _camera.depthTextureMode = depthTextureMode;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (passes != null)
        {
            foreach(var material in passes)
                Graphics.Blit(source, destination, material);

        }
        else
            Graphics.Blit(source, destination);
    }
}
