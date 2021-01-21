using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraEffect : MonoBehaviour {

    private Camera _camera;

    public DepthTextureMode depthMode = DepthTextureMode.Depth;
    public Material[] materials;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = depthMode;
    }
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        foreach(Material material in materials)
            Graphics.Blit(source, destination, material);
    }
}
