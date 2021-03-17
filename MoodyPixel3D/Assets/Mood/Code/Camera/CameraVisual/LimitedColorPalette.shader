Shader "Long Hat House/Pixel Art/LimitedColorPalette"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _DitheringTex ("Dithering Texture", 2D) = "gray" {}
        _DitheringForce ("Dithering Force", Float) = 0
        _DitheringNeutral ("Dithering Neutral", Float) = -.5
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
            fixed4 _MainTex_TexelSize;
            sampler2D _DitheringTex;
            fixed4 _DitheringTex_TexelSize;
            fixed _DitheringForce;
            fixed _DitheringNeutral;
			int _MaxColors;
			fixed4 _Colors[256];

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed2 mult = fixed2(_MainTex_TexelSize.z / _DitheringTex_TexelSize.z ,_MainTex_TexelSize.w / _DitheringTex_TexelSize.w);
                fixed4 noise = tex2D(_DitheringTex, ComputeScreenPos(i.vertex));
                //fixed4 noise = tex2D(_DitheringTex, i.uv * mult + fixed2(_Time.x * 0, 0));
				int x = 0;
				int choice = 0;
				int maxChoice = -1;
				float minDist = FLT_MAX;
				float maxDist = 0;
                //return noise;
				for (x = 0; x < _MaxColors; x++)
				{
					float dist = Distance(_Colors[x], col + (noise + _DitheringNeutral) * _DitheringForce);

					if (dist <= minDist) 
					{
						minDist = dist;
						choice = x;
					}
					else if (dist > maxDist)
					{
						maxDist = dist;
					}
				}
                return _Colors[choice];// + (noise - 0.5) * _DitheringForce;// max(-.25, min(.25, _DitheringForce * .001));
            }

			
            ENDCG
        }
    }
}
