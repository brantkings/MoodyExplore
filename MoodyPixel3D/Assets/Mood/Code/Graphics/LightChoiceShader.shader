Shader "Custom/LightChoiceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf WrapLambert fullforwardshadows noambient nofog

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        fixed4 _Color;
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        

        half4 LightingWrapLambert (inout SurfaceOutput s, half3 lightDir, half atten) {
            //return LightingStandard(s, lightDir, atten);
            half NdotL = dot (s.Normal, lightDir);

            //float4 debugVal = debug[0];
            //debugVal = float4(min(debugVal.x, atten), max(debugVal.y, atten), 1, 1);
            
            return smoothstep(0.25,0.4,atten) * _LightColor0.a;
            //return atten;
            return step(0, NdotL * atten);
            //float towardsLightChange = fwidth(towardsLight);

            //float attenChange = fwidth(atten) * 0.5;
            //float shadow = smoothstep(0.1 - attenChange, 0.1 + attenChange, atten);


            //return smoothstep(0.5-towardsLightChange, 0.5+ towardsLightChange, NdotL) * atten;
            
            //half diff = NdotL * 0.5 + 0.5;
            //half diff = NdotL;
            half4 c;
            //c.rgb = s.Albedo * (diff * atten);
            //c.rgb = _LightColor0.rgb * (diff * atten * atten);
            //c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * atten);
            //c.rgb = s.Albedo;
            //c.rgb = (diff * atten * atten);
            //c.rgb = atten;
            //c.rgb = pow(atten * diff,4);
            c.a = s.Alpha;
            return c;
        }


        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed med = (_Color.r + _Color.g + _Color.b) /3;
            o.Albedo = fixed4(0,0,0,0);
            o.Emission = fixed4(-1,-1,-1,-1);
            //o.Albedo = fixed3(med,med,med);
            // o.Albedo = _Color;
            //o.Normal = fixed3(1,1,1);
            //o.Emission = -_Color * 0.1;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
