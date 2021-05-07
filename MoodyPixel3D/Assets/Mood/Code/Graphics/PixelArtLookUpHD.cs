using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Long Hat House/PixelArtLookUpHD")]
public sealed class PixelArtLookUpHD : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    private static Vector4 SCALE_BIAS = new Vector4(1f, 1f, 0f, 0f);
    public ClampedFloatParameter blend = new ClampedFloatParameter(1f, 0f, 1f);
    public IntParameter screenHeight = new IntParameter(256);
    public IntParameter materialPassInTheEnd = new IntParameter(1);
    public BoolParameter tryChange = new BoolParameter(false);


    Material pixelizeMaterial;
    RTHandleSystem rtSystem;

    public bool IsActive() => pixelizeMaterial != null && screenHeight.value > 0;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > HDRP Default Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    const string kShaderName = "Long Hat House/HD/Screen Effect/Pixelize";
    int shaderPixelizeTextureParameterID;
    int shaderPixelizeSampleParameterID;

    MaterialPropertyBlock prop;

    public override void Setup()
    {
        Debug.LogFormat($"Oi tudo bem!???");
        rtSystem = new RTHandleSystem();
        rtSystem.Initialize(Screen.width, Screen.height, false, MSAASamples.None);
        shaderPixelizeTextureParameterID = Shader.PropertyToID("_PixelatedRT");
        shaderPixelizeSampleParameterID = Shader.PropertyToID("_PixelatedTexture");
        prop = new MaterialPropertyBlock();
        //RTHandles.Initialize(Screen.width, Screen.height, false, MSAASamples.None);
        if (Shader.Find(kShaderName) != null)
            pixelizeMaterial = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume PixelArtLookUpHD is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        /*if (m_Material == null)
        {
            return;
        }*/

        float cameraHeight = camera.actualHeight;
        float cameraWidth = camera.actualWidth;
        float factor = Mathf.Lerp(1f, screenHeight.value / cameraHeight, blend.value);
        int height = Mathf.RoundToInt((float)cameraHeight * factor);
        int width = Mathf.RoundToInt((float)cameraWidth * factor);

        //Debug.LogFormat($"{width} and {height} for {camera.camera}!");


        cmd.BeginSample("Low Scale Sampling");
        pixelizeMaterial.SetTexture("_InputTexture", source);
        //m_Material.SetTexture("_MainTex", source);

        int rtID = Shader.PropertyToID("PixelArtLookUp");
        RTHandle rt = RTHandles.Alloc(scaleFactor: Vector2.one * 1f);// colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat);//, filterMode: FilterMode.Point, wrapMode: TextureWrapMode.Clamp, dimension: TextureDimension.Tex2D);
        //cmd.Blit(source, rtID);
        //cmd.Blit(rtID, destination, m_Material);
        //HDUtils.BlitCameraTexture(cmd, source, rtID);
        /*CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.All);
        HDUtils.BlitQuad(cmd, source, SCALE_BIAS, SCALE_BIAS, 0, false);
        CoreUtils.SetRenderTarget(cmd, destination, ClearFlag.None);
        HDUtils.BlitQuad(cmd, rt, SCALE_BIAS, SCALE_BIAS, 0, false);*/

        //CoreUtils.SetRenderTarget(cmd, rt, ClearFlag.None);
        //HDUtils.Blit(cmd, source, SCALE_BIAS, 0, false);
        //cmd.Blit(source, rt);
        //cmd.Blit(rt, destination);
        if(tryChange.value)
        {
            HDUtils.BlitCameraTexture(cmd, source, rt);
            //HDUtils.BlitCameraTexture(cmd, rt, destination);
            CoreUtils.SetRenderTarget(cmd, destination);
            HDUtils.BlitTexture(cmd, rt, SCALE_BIAS, 100f, false);
        }
        else
        {
            HDUtils.DrawFullScreen(cmd, pixelizeMaterial, rt, prop, 0);
            prop.SetTexture(Shader.PropertyToID("_PixelatedRT"), rt);
            prop.SetTexture(Shader.PropertyToID("_PixelatedTexture"), rt);
            pixelizeMaterial.SetTexture(shaderPixelizeTextureParameterID, rt);
            pixelizeMaterial.SetTexture(shaderPixelizeSampleParameterID, rt);
            HDUtils.DrawFullScreen(cmd, pixelizeMaterial, destination, prop, materialPassInTheEnd.value);
            //HDUtils.BlitCameraTexture(cmd, source, destination, pixelizeMaterial, materialPassInTheEnd.value);
        }
        rt.Release();
        cmd.EndSample("Low Scale");
    }

    public override void Cleanup()
    {
        rtSystem.Dispose();
        CoreUtils.Destroy(pixelizeMaterial);
        
    }
}
