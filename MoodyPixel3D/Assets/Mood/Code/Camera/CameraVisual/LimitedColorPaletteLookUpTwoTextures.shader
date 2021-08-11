Shader "Long Hat House/Pixel Art/Limited Color LookUp Two Textures"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _LookUpTableI("IlluminatedPalette", 3D) = "white" {}
        _LookUpTableN("NotIlluminatedPalette", 3D) = "white" {}
        _IlluminatedTex("Choice Render Texture", 2D) = "white" {}
        _DitheringTex ("Dithering Texture", 2D) = "gray" {}
        _DitheringForce ("Dithering Force", Float) = 0
        _DitheringNeutral ("Dithering Neutral", Float) = -.5
        _TextureSize ("Round", Float) = 16
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


			#define FLT_MAX 3.402823466e+38
			#define FLT_MIN 1.175494351e-38
			#define DBL_MAX 1.7976931348623158e+308
			#define DBL_MIN 2.2250738585072014e-308
            #define SHADOWS_SCREEN

            #include "UnityCG.cginc"
            #include "UnityShadowLibrary.cginc"
			#include "CIELABColor.cginc"
            #include "Lighting.cginc"
			#include "AutoLight.cginc"

            /*
            #ifndef AUTOLIGHT_FIXES_INCLUDED
                #define AUTOLIGHT_FIXES_INCLUDED
 
                #include "HLSLSupport.cginc"
                #include "UnityShadowLibrary.cginc"
 
                // Problem 1: SHADOW_COORDS - undefined identifier.
                // Why: Using SHADOWS_DEPTH without SPOT.
                // The file AutoLight.cginc only takes into account the case where you use SHADOWS_DEPTH + SPOT (to enable SPOT just add a Spot Light in the scene).
                // So, if your scene doesn't have a Spot Light, it will skip the SHADOW_COORDS definition and shows the error.
                // Now, to workaround this you can:
                // 1. Add a Spot Light to your scene
                // 2. Use this CGINC to workaround this scase.  Also, you can copy this in your own shader.
                #if defined (SHADOWS_DEPTH) && !defined (SPOT)
                    #define SHADOW_COORDS(idx1) unityShadowCoord2 _ShadowCoord : TEXCOORD##idx1;
                #endif
 
 
                // Problem 2: _ShadowCoord - invalid subscript.
                // Why: nor Shadow screen neighter Shadow Depth or Shadow Cube and trying to use _ShadowCoord attribute.
                // The file AutoLight.cginc defines SHADOW_COORDS to empty when no one of these options are enabled (SHADOWS_SCREEN, SHADOWS_DEPTH and SHADOWS_CUBE),
                // So, if you try to call "o._ShadowCoord = ..." it will break because _ShadowCoord isn't an attribute in your structure.
                // To workaround this you can:
                // 1. Check if one of those defines actually exists in any place where you have "o._ShadowCoord...".
                // 2. Use the define SHADOWS_ENABLED from this file to perform the same check.
                #if defined (SHADOWS_SCREEN) || defined (SHADOWS_DEPTH) || defined (SHADOWS_CUBE)
                    #define SHADOWS_ENABLED
                #endif
            #endif
            */

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				//SHADOW_COORDS(5)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _LightSample;
            sampler3D _LookUpTableI;
            fixed4 _ColorAddI;
            fixed4 _ColorMulI;
            fixed _TextureSize;
            float4 _LookUpTableI_TexelSize;
            sampler3D _LookUpTableN;
            fixed4 _ColorAddN;
            fixed4 _ColorMulN;
            float4 _LookUpTableN_TexelSize;
            sampler2D _IlluminatedTex;
            fixed4 _MainTex_TexelSize;
            sampler2D _DitheringTex;
            fixed4 _DitheringTex_TexelSize;
            fixed _DitheringForceI;
            fixed _DitheringForceN;
            fixed _DitheringNeutral;

            float4 frag (v2f i) : SV_Target
            {

                float4 col = tex2D(_MainTex, i.uv);
                //fixed2 mult = fixed2(_MainTex_TexelSize.z / _DitheringTex_TexelSize.z ,_MainTex_TexelSize.w / _DitheringTex_TexelSize.w);
                float4 noise = tex2D(_DitheringTex, ComputeScreenPos(i.vertex));
                
				//float3 shadowCoord = i._ShadowCoord.xyz / i._ShadowCoord.w;
				//float4 shadowmap = tex2D(_LightSample, shadowCoord.xy);

                float4 iChoice = tex2D(_LightSample, i.uv);

                float4 colI = col + (noise + _DitheringNeutral) * _DitheringForceI;
                if(_TextureSize>0)
                    colI = round(colI  * _TextureSize) / _TextureSize;
                float4 colN = col + (noise + _DitheringNeutral) * _DitheringForceN;
                if(_TextureSize>0)
                    colN = round(colN  * _TextureSize) / _TextureSize;
                float4 colorI = tex3D(_LookUpTableI, colI);
                colorI = (colorI + _ColorAddI) * _ColorMulI;
                float4 colorN = tex3D(_LookUpTableN, colN);
                colorN = (colorN + _ColorAddN) * _ColorMulN;
                //return iChoice;
                //return smoothstep(0, 1, iChoice);
                //return shadowmap;
                
                //iChoice = smoothstep(0, 0.01, iChoice);
                //return smoothstep(0.45, 0.5, iChoice);
                return lerp(colorN, colorI, smoothstep(0,1,iChoice));
                //return tex3D(_LookUpTable, col);
            }

			
            ENDCG
        }
    }
}
