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

    public override void Render(PostProcessRenderContext context)
    {
        InitMaterial(settings.material.value);
        int height = settings.height.value;
        int width = Mathf.FloorToInt(Camera.main.aspect * height);
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
