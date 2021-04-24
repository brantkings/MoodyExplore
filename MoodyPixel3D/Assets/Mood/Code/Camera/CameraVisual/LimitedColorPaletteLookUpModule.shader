﻿Shader "Long Hat House/Pixel Art/Limited Color Look Up Exact"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _LookUpTable("Texture", 3D) = "white" {}
        _DitheringTex ("Dithering Texture", 2D) = "gray" {}
        _DitheringForce ("Dithering Force", Float) = 0
        _DitheringNeutral ("Dithering Neutral", Float) = -.5
        _AAA ("AAA", Float) = 1
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

            #include "UnityCG.cginc"
			#include "CIELABColor.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler3D _LookUpTable;
            fixed4 _LookUpTable_TexelSize;
            fixed4 _MainTex_TexelSize;
            sampler2D _DitheringTex;
            fixed4 _DitheringTex_TexelSize;
            fixed _DitheringForce;
            fixed _DitheringNeutral;
            fixed _AAA;


            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                //fixed2 mult = fixed2(_MainTex_TexelSize.z / _DitheringTex_TexelSize.z ,_MainTex_TexelSize.w / _DitheringTex_TexelSize.w);
                float4 noise = tex2D(_DitheringTex, ComputeScreenPos(i.vertex));
                float4 colorPosition = col + (noise + _DitheringNeutral) * _DitheringForce;
                float texelSize = _LookUpTable_TexelSize.x;
                float halfTexelSize = texelSize / 2.;
                float4 colorPositionIndex = colorPosition / texelSize;
                float4 colorPositionRemainder = colorPosition % texelSize;
                for(int i = 0;i<3;i++)
                {
                    if(colorPositionRemainder[i] > halfTexelSize)
                    {
                       colorPosition[i] += colorPositionRemainder[i] * _AAA;
                    }
                    else
                    {
                       colorPosition[i] -= colorPositionRemainder[i] * _AAA;
                    }
                }
                //colorPosition = colorPositionIndex * texelSize;
                return tex3D(_LookUpTable, colorPosition);
                //return float4(1,1,0,0);
                //return tex3D(_LookUpTable, col);
            }

			
            ENDCG
        }
    }
}
