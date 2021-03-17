Shader "Long Hat House/WallShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _GroundTex ("Ground Pattern", 2D) = "white" {}
        _WallTex ("Wall Pattern", 2D) = "white" {}
        _DitheringTex ("Dithering Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AmountForce ("DitheringForce", Float) = 0.
    }
    CGINCLUDE
    sampler2D _WallTex;
    sampler2D _GroundTex;
    sampler2D _DitheringTex;
    float4 _DitheringTex_TexelSize;
    half _Glossiness;
    half _Metallic;
    fixed4 _Color;
    fixed4 PlayerPosition;
    float _AmountForce;

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
        if(showEverythingBelow < 0)
        {
            clip(isWall * (distanceToShowWall - 1.) + clipSaveValue);
        }
    }
    ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry-2" }
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
            fixed4 ground = tex2D (_GroundTex, IN.worldPos.xz) * _Color;
            fixed4 wallA = tex2D (_WallTex, IN.worldPos.xy) * _Color;
            fixed4 wallB = tex2D (_WallTex, IN.worldPos.zy) * _Color;

            fixed4 c = ground * abs(IN.worldNormal.y) + wallA * abs(IN.worldNormal.z) + wallB * abs(IN.worldNormal.x);
            o.Albedo = c.rgb;

            float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
            float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitheringTex_TexelSize.xy;
            float ditherValue = tex2D(_DitheringTex, ditherCoordinate * _AmountForce);
            
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
            float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
            float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitheringTex_TexelSize.xy;
            float ditherValue = tex2D(_DitheringTex, ditherCoordinate * _AmountForce);

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
