using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(PixelArtLookUpRender), PostProcessEvent.AfterStack, "Long Hat House/Pixel Art Look Up")]
public class PixelArtLookUpSettings : PostProcessEffectSettings
{
    public IntParameter height = new IntParameter();
    public ParameterOverride<FilterMode> mode = new ParameterOverride<FilterMode>(FilterMode.Point);
    public ParameterOverride<Shader> shader = new ParameterOverride<Shader>();
    public ParameterOverride<Texture3D> textureIlluminated = new ParameterOverride<Texture3D>();
    public ParameterOverride<Texture3D> textureNotIlluminated = new ParameterOverride<Texture3D>();
    public BoolParameter changeBefore = new BoolParameter();
    public FloatParameter ditheringForce = new FloatParameter();
    public FloatParameter ditheringNeutral = new FloatParameter();
    public ParameterOverride<Texture2D> ditheringTexture = new ParameterOverride<Texture2D>();
    public ParameterOverride<string> customBufferName = new ParameterOverride<string>("LightTexture");
    public ParameterOverride<string> textureParameterID = new ParameterOverride<string>("_LightSample");
    public ParameterOverride<Shader> quickTestShader = new ParameterOverride<Shader>();
    public BoolParameter onlyRenderIlluminationTexture = new BoolParameter();

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

    private Material mat;

    public int index;
    public static int indexGroup;

    public override void Init()
    {
        base.Init();
        Debug.LogFormat("Init: {3} {0} {1} {2}", settings, settings?.shader, settings?.shader?.value, this);
        if(settings.shader?.value != null)
        {
            mat = new Material(settings.shader.value);
            if (settings.ditheringTexture?.value != null)
            {
                mat.SetTexture("_DitheringTex", settings.ditheringTexture.value);
            }
        }

        Debug.LogFormat("End Init: mat is {0}! OK!", mat);
        index = indexGroup++;
    }

    public override void Release()
    {
        base.Release();
    }

    protected virtual bool InitMaterial(Material mat, PostProcessRenderContext context)
    {
        mat.SetTexture("_LookUpTableI", settings.textureIlluminated.value);
        mat.SetTexture("_LookUpTableN", settings.textureNotIlluminated.value);
        mat.SetFloat("_DitheringForce", settings.ditheringForce.value);
        mat.SetFloat("_DitheringNeutral", settings.ditheringNeutral.value);

        //Debug.LogFormat("Buffer name {0} and texture parameter ID {1} and camera {2}", settings?.customBufferName?.value, settings?.textureParameterID?.value, context.camera);
        if (!string.IsNullOrEmpty(settings.customBufferName.value) && !string.IsNullOrEmpty(settings.textureParameterID.value))
        {
            RenderTexture tex = GlobalCameraBuffer.GetBuffer(context.camera, settings.customBufferName.value);
            if (tex != null)
            {
                //Debug.LogFormat(tex, "Found the texture {0} for {1} ({2}/{3})", tex, context.camera, index, indexGroup);
                mat.SetTexture(Shader.PropertyToID(settings.textureParameterID.value), tex);

                if(settings.onlyRenderIlluminationTexture)
                {
                    context.command.Blit(tex, context.destination);
                    return false;
                }
            }
            else
            {
                //Debug.LogFormat("Didn't found texture for {0}", context.camera);
            }
        }

        return true;
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
        int height = settings.height.value;
        int width = Mathf.FloorToInt(Camera.main.aspect * height);

        if (settings.quickTestShader.value != null)
        {
            context.camera.RenderWithShader(settings.quickTestShader, null);
            return;
        }

        if (mat != null)
        {
            if(!InitMaterial(mat, context)) return;
        }



        


        RenderTexture temp = RenderTexture.GetTemporary(width, height);
        RenderTexture.ReleaseTemporary(temp);
        temp.filterMode = settings.mode;
        if(settings.changeBefore.value)
        {
            context.command.Blit(context.source, temp);
            BlitLimitingColors(context, mat, temp, temp);
        }
        else
        {
            BlitLimitingColors(context, mat, context.source, temp);
        }
        context.command.Blit(temp, context.destination);
    }

    private void BlitLimitingColors(PostProcessRenderContext context, Material m, UnityEngine.Rendering.RenderTargetIdentifier source, UnityEngine.Rendering.RenderTargetIdentifier destination)
    {
        if (m != null)    
            context.command.Blit(source, destination, m);
        else
            context.command.Blit(source, destination);
    }
}
