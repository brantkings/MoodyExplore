using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode()]
public class GlobalPropertyRenderer : MonoBehaviour
{
    public string globalShaderProperty = "_RenderTex";
    public Camera toCopyFrom;
    public Shader shaderToUse;
    public string shaderTag;

    RenderTexture renderTexture;
    Camera myCam;
    private void Awake()
    {
        myCam = GetComponent<Camera>();
        myCam.enabled = false;
    }

    private void OnEnable()
    {
        if (toCopyFrom != null) myCam.CopyFrom(toCopyFrom);
        renderTexture = RenderTexture.GetTemporary(myCam.pixelWidth, myCam.pixelHeight, 16);
        renderTexture.name = name + globalShaderProperty;
        myCam.targetTexture = renderTexture;
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(renderTexture);
    }


    private void LateUpdate()
    {
#if UNITY_EDITOR
        OnEnable();
#endif

        if (shaderToUse != null) myCam.RenderWithShader(shaderToUse, string.IsNullOrEmpty(shaderTag)? null : shaderTag);
        else myCam.Render();
        renderTexture.SetGlobalShaderProperty(globalShaderProperty);

#if UNITY_EDITOR
        OnDisable();
#endif
    }
}
