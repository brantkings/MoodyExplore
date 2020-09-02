Shader "LongHatHouse/GrayscaleUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _ShadowColor("ShadowColor", Color) = (0,0,1,1)
        _ShadowIntensity("ShadowIntensity", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf GrayShadow fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        
        fixed4 _ShadowColor;
        half _ShadowIntensity;
  
          half4 LightingGrayShadow (SurfaceOutput s, half3 lightDir, half atten) {
              half NdotL = dot (s.Normal, lightDir);
              half4 c;
              float gray = (s.Albedo.r + s.Albedo.g + s.Albedo.b) / 3.;
              gray = gray * (abs(dot(s.Normal, half3(0,1,0))) + abs(dot(s.Normal, half3(1,0,0))) + abs(dot(s.Normal, half3(0,0,1))));
              float lightValue = (NdotL * atten);
              c.rgb = s.Albedo * _LightColor0.rgb * lightValue;
              c.rgb += _ShadowColor * gray * _ShadowIntensity * (1 - atten);
              c.a = s.Alpha;
              return c;
          }

        struct Input
        {
            float2 uv_MainTex;
        };

        //half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        
        ENDCG
        
    }
    FallBack "Diffuse"
}
