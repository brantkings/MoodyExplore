using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelArtCamera : MonoBehaviour
{
    private int _width;
    public int heightResolution = 144;

    public Material screenEffect;
    public string screenEffectColorArray = "_Colors";
    public string screenEffectMaxColorInt = "_MaxColors";
    public ColorPalette palette;

    private Camera cam;

    protected void Start()
    {
        cam = GetComponent<Camera>();
        SetMaterial();
    }

    private void SetMaterial()
    {
        if (palette != null && screenEffect != null)
        {
            palette.SetMaterial(screenEffect, screenEffectColorArray, screenEffectMaxColorInt);
        }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    void Update()
    {
        float ratio = cam.aspect;
        _width = Mathf.RoundToInt(heightResolution * ratio);
        SetMaterial();
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
#if UNITY_EDITOR
        SetMaterial();
#endif
        source.filterMode = FilterMode.Point;
        RenderTexture buffer = RenderTexture.GetTemporary(_width, heightResolution, -1);
        buffer.filterMode = FilterMode.Point;
        Graphics.Blit(source, buffer);
        if(screenEffect)
            Graphics.Blit(buffer, destination, screenEffect);
        else
            Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }
}
