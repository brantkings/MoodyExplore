Shader "Long Hat House/WallShader"
{
    Properties
    {
        _ColorGround ("ColorGround", Color) = (1,1,1,1)
        _ColorWall ("ColorWall", Color) = (1,1,1,1)
        _GroundTex ("Ground Pattern", 2D) = "white" {}
        _WallTex ("Wall Pattern", 2D) = "white" {}
        _DitheringTex ("Dithering Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AmountForce ("DitheringForce", Float) = 0.
        _FadeValue ("FadeValue", Range(0,1)) = 1
        _FadeSucceptiveness("Fade Succeptiveness", Float) = 0
    }

    CGINCLUDE
    sampler2D _WallTex;
    fixed4 _WallTex_ST;
    sampler2D _GroundTex;
    fixed4 _GroundTex_ST;
    sampler2D _DitheringTex;
    float4 _DitheringTex_TexelSize;
    half _Glossiness;
    half _Metallic;
    fixed4 _ColorGround;
    fixed4 _ColorWall;
    fixed4 PlayerPosition;
    float _AmountForce;
    fixed _FadeValue;
    fixed _FadeSucceptiveness;

    struct Input
    {
        float2 uv_MainTex;
        float3 worldPos;
        float4 screenPos;
        float3 worldNormal;
        float3 viewDir;
    };

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    // #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
    UNITY_INSTANCING_BUFFER_END(Props)

    //For not lightining
    half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           return fixed4(0,0,0,0);//half4(s.Albedo, s.Alpha);
    }

    inline fixed3 project(fixed3 a, fixed3 onto)
    {
        float lOnto = length(onto);
        return dot(a,onto) * normalize(onto) / lOnto;  
    }

    inline fixed2 project2d(fixed2 a, fixed2 onto)
    {
        float lOnto = length(onto);
        return dot(a,onto) * normalize(onto) / lOnto;  
    }

    inline void ClipParts(Input IN, float clipSaveValue)
    {
        fixed2 playerDist = PlayerPosition.xz - _WorldSpaceCameraPos.xz;
        fixed2 pointDist = IN.worldPos.xz - _WorldSpaceCameraPos.xz;
        //Clip everything below player
        fixed playerHeightDistance = PlayerPosition.y - IN.worldPos.y;
        float showEverythingBelow = playerHeightDistance + .5 - clipSaveValue;
        float distanceToShowWall = (length(pointDist) - length(playerDist));
        float isWall = length(IN.worldNormal.xz);
        clip((_FadeValue - 1) + (clipSaveValue * _FadeSucceptiveness));
        if(showEverythingBelow < 0)
        {
            clip(isWall * (distanceToShowWall - 1.) + clipSaveValue);
        }
    }

    inline float GetDitherValue(float2 screenPos, sampler2D dTex, float4 dTex_TexelSize)
    {
        //* _ScreenParams.xy 
       float2 ditherCoordinate = screenPos * (dTex_TexelSize.xy * 0.25) * _ScreenParams.xy;
       return (tex2D(dTex, ditherCoordinate)) * _AmountForce;
        //return (tex2D(dTex, screenPos.xy * screenPos.w) - .5) * _AmountForce;
    }
    ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry-2" "Fader" = "True"}
        //Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        

        LOD 200
        //Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite On
        ZTest LEqual
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed2 uvGround = TRANSFORM_TEX(IN.worldPos.xz, _GroundTex);
            fixed2 uvWallA = TRANSFORM_TEX(IN.worldPos.xy, _WallTex);
            fixed2 uvWallB = TRANSFORM_TEX(IN.worldPos.zy, _WallTex);
            fixed4 ground = tex2D (_GroundTex, uvGround) * _ColorGround;
            fixed4 wallA = tex2D (_WallTex, uvWallA) * _ColorWall;
            fixed4 wallB = tex2D (_WallTex, uvWallB) * _ColorWall;

            fixed4 c = ground * abs(IN.worldNormal.y) + wallA * abs(IN.worldNormal.z) + wallB * abs(IN.worldNormal.x);
            o.Albedo = c.rgb;

            float ditherValue = GetDitherValue(IN.screenPos.xy / IN.screenPos.w, _DitheringTex, _DitheringTex_TexelSize);
            //float ditherValue = GetDitherValue(ComputeScreenPos(IN.screenPos), _DitheringTex, _DitheringTex_TexelSize);
            
            ClipParts(IN, ditherValue);

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
        
        LOD 200
        //Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        ZWrite On
        ZTest LEqual
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float ditherValue = GetDitherValue(IN.screenPos.xy / IN.screenPos.w, _DitheringTex, _DitheringTex_TexelSize);
            //float ditherValue = GetDitherValue(ComputeScreenPos(IN.screenPos), _DitheringTex, _DitheringTex_TexelSize);

            o.Albedo = fixed3(0.,0.,0.);
            ClipParts(IN, ditherValue);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
