using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(PixelArtRender), PostProcessEvent.AfterStack, "Long Hat House/Pixel Art Palette")]
public class PixelArtSettings : PostProcessEffectSettings
{
    public IntParameter height = new IntParameter();
    public ParameterOverride<FilterMode> mode = new ParameterOverride<FilterMode>(FilterMode.Point);
    public ParameterOverride<Material> material = new ParameterOverride<Material>();
    public ParameterOverride<ColorPalette> palette = new ParameterOverride<ColorPalette>();
    public BoolParameter changeBefore = new BoolParameter();

    public override bool IsEnabledAndSupported(PostProcessRenderContext context)
    {
        return base.IsEnabledAndSupported(context) && height.value > 0 && HasValidFunctionality();
    }

    private bool HasValidFunctionality()
    {
        if (material != null && material.value == null) return true;
        else return palette.value != null;
    }
}


public class PixelArtRender : PostProcessEffectRenderer<PixelArtSettings>
{
    public override void Init()
    {
        base.Init();
    }

    protected virtual void InitMaterial(Material mat)
    {
        settings.palette.value.SetMaterial(mat, "_Colors", "_MaxColors");
    }

    public override void Render(PostProcessRenderContext context)
    {
        if (settings.palette.value != null && settings.material.value != null)
            InitMaterial(settings.material.value);
        int height = settings.height.value;
        int width = Mathf.FloorToInt(context.camera.aspect * height);
            
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
        if (settings.material.value != null || settings.palette.value != null)
            context.command.Blit(source, destination, settings.material.value);
        else
            context.command.Blit(source, destination);
    }
}
