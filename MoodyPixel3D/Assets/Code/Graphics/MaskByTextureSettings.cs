using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


[PostProcess(typeof(MaskByTextureRender), PostProcessEvent.AfterStack, "Long Hat House/Mask by Texture")]
public class MaskByTextureSettings : PostProcessEffectSettings
{
    public ParameterOverride<RenderTexture> mask = new ParameterOverride<RenderTexture>();
    public ParameterOverride<Material> material = new ParameterOverride<Material>();
}

public class MaskByTextureRender : PostProcessEffectRenderer<MaskByTextureSettings>
{
    public override void Render(PostProcessRenderContext context)
    {
        if(settings.material.value != null && settings.mask.value != null)
        {
            settings.material.value.SetTexture("_MaskTex", settings.mask.value);
            context.command.Blit(context.source, context.destination, settings.material);
        }
        else
        {
            context.command.Blit(context.source, context.destination);
        }
    }
}

