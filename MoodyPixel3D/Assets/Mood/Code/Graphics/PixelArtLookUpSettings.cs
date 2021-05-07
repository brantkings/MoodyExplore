using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(PixelArtLookUpRender), PostProcessEvent.AfterStack, "Long Hat House/Pixel Art Look Up")]
public class PixelArtLookUpSettings : PostProcessEffectSettings
{
    public IntParameter height = new IntParameter();
    public ParameterOverride<FilterMode> mode = new ParameterOverride<FilterMode>(FilterMode.Point);
    public ParameterOverride<Material> material = new ParameterOverride<Material>();
    public BoolParameter changeBefore = new BoolParameter();
    public ParameterOverride<Shader> lightShader = new ParameterOverride<Shader>();

    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return base.IsEnabledAndSupported(context) && HasValidFunctionality();
    }

    private bool HasValidFunctionality()
    {
        return height.value > 0;
    }
}


public class PixelArtLookUpRender : PostProcessEffectRenderer<PixelArtLookUpSettings>
{

    public override void Init()
    {
        base.Init();
    }

    protected virtual void InitMaterial(Material mat)
    {

    }

    private void SetupCamera(Camera c, PostProcessRenderContext context)
    {
        if (c == null) c = GameObject.Instantiate(context.camera);
        c.renderingPath = RenderingPath.Forward;
        c.clearFlags = CameraClearFlags.Color;
        c.backgroundColor = Color.black;
    }

    public override void Render(PostProcessRenderContext context)
    {
        InitMaterial(settings.material.value);
        int height = settings.height.value;
        int width = Mathf.FloorToInt(Camera.main.aspect * height);

        if (settings.lightShader.value != null)
        {
            RenderTexture lightTexture = RenderTexture.GetTemporary(context.screenWidth, context.screenHeight, 16);
            SetupCamera(context.camera, context);
            //context.camera.targetTexture = lightTexture;
            //context.command.SetShadowSamplingMode(lightTexture, UnityEngine.Rendering.ShadowSamplingMode.CompareDepths);
            context.command.SetRenderTarget(lightTexture);
            UnityEngine.Rendering.CommandBuffer cameraBuffer = new UnityEngine.Rendering.CommandBuffer();
            cameraBuffer.SetRenderTarget(lightTexture);
            context.camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.BeforeDepthTexture, cameraBuffer);
            context.camera.RenderWithShader(settings.lightShader, null);

            //context.command.ClearRenderTarget(true, true, Color.black);
            //context.command.Blit(lightTexture, context.destination);
            //context.camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(lightTexture);
            return;
        }

        RenderTexture temp = RenderTexture.GetTemporary(width, height);
        temp.filterMode = settings.mode;
        if(settings.changeBefore.value)
        {
            context.command.Blit(context.source, temp);
            BlitLimitingColors(context, temp, temp);
        }
        else
        {
            BlitLimitingColors(context, context.source, temp);
        }
        context.command.Blit(temp, context.destination);
        RenderTexture.ReleaseTemporary(temp);
    }

    private void BlitLimitingColors(PostProcessRenderContext context, UnityEngine.Rendering.RenderTargetIdentifier source, UnityEngine.Rendering.RenderTargetIdentifier destination)
    {
        if (settings.material.value != null)    
            context.command.Blit(source, destination, settings.material.value);
        else
            context.command.Blit(source, destination);
    }
}
