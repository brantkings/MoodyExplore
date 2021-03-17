Shader "Long Hat House/Pixel Art/Limited Color Palette Better Comparison By Hue"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
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
			int _MaxColors;
			fixed4 _ComparingColors[256];
			fixed _ColorIndexes[256];
			fixed4 _Colors[256];
			fixed _Luminances[256];
			fixed _MinDelta;
			fixed _MinLuminance;
			fixed _MaxLuminance;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				int x = 0;
				int y = 0;
				float3 colLab = RGBtoLAB(col);
				int choice = 0;
				int maxChoice = -1;
				fixed luminance = colLab[0];
				float dist = 0.;
				float minDist = FLT_MAX;
				float maxDist = 0;
				
				//if(luminance > _MinLuminance && luminance < _MaxLuminance)
				//{
					for (x = 0; x < _MaxColors; x++)
					{
						float3 compColLab = RGBtoLAB(_ComparingColors[x]);
						dist = sqrt(DistanceNoLuminanceAlreadyLAB(colLab, compColLab)); //This gets the dist and luminance, has out values
					
						if(dist < _MinDelta)
						{
							if (dist < minDist) 
							{
								minDist = dist;
								choice = x;
								
							}
						}
						if (dist > maxDist)
						{
							maxDist = dist;
						}
					}
				//}

				//return _ComparingColors[choice];

				
				float maxLuminance = 100.;
				int indexNow = _ColorIndexes[choice];
				int indexNext = _ColorIndexes[choice + 1];
				int lengthOfColors = indexNext - indexNow;
				
				//Test if luminance is working
				//int lengthOfColors = 4;
				//fixed lum = (fixed)whereLum / lengthOfColors;
				//return fixed4(lum,lum,lum,1.);


				//When colors where evenly distributed by all range of luminance, it didnt work
				//fixed lumStep = maxLuminance / lengthOfColors;
				//int whereLum = round(luminance / lumStep);
                //return _Colors[min(indexNow + whereLum, indexNext-1)];

				//RE CONTEXTUALIZE SAME VARIABLES
				float minDeltaLum = FLT_MAX;
				for (x = indexNow; x < indexNext; x++)
				{
					float dist = abs(_Luminances[x] - luminance);

					if(dist < minDeltaLum)
					{
						minDeltaLum = dist;
						choice = x;
					}
				}
				
                return _Colors[choice];

            }

			
            ENDCG
        }
    }
}
