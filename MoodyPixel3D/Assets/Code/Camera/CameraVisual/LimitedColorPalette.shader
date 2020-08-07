Shader "Long Hat House/Pixel Art/LimitedColorPalette"
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
			fixed4 _Colors[256];

			float3 RGBtoXYZ(in float3 i)
			{
				if(i.r > 0.04045)
				{
					i.r = pow(((i.r + 0.055) / 1.055) , 2.4);
				}
				else 
				{
					i.r = i.r / 12.92;
				}

				if (i.g > 0.04045)
				{
					i.g = pow(((i.g + 0.055) / 1.055), 2.4);
				}
				else 
				{
					i.g = i.g / 12.92;
				}

				if (i.b > 0.04045)
				{
					i.b = pow(((i.b + 0.055) / 1.055), 2.4);
				}
				else 
				{
					i.b = i.b / 12.92;
				}

				i.r *= 100;
				i.g *= 100;
				i.b *= 100;

				float3 output;

				output.x = (i.r * 0.4124 + i.g * 0.3576 + i.b * 0.1805);
				output.y = (i.r * 0.2126 + i.g * 0.7152 + i.b * 0.0722);
				output.z = (i.r * 0.0193 + i.g * 0.1192 + i.b * 0.9505);

				return output;
			}

			float3 XYZtoLAB(in float3 i)
			{
				float3 reference = float3(94.811, 100.000, 107.304); //Daylight, sRGB, Adobe-RGB according to http://www.easyrgb.com/en/math.php#text2

				float ka = (175.0 / 198.04) * (reference.y + reference.x);
				float kb = (70.0 / 218.11) * (reference.y + reference.z);

				float3 lab;
				lab.r = 100.0 * sqrt(i.y / reference.y);
				lab.g = ka * (((i.x / reference.x) - (i.y / reference.y)) / sqrt(i.y / reference.y));
				lab.b = kb * (((i.y / reference.y) - (i.z / reference.z)) / sqrt(i.y / reference.y));

				return lab;
				

				/*float3 var;

				var.x = i.x / reference.x;
				var.y = i.y / reference.y;
				var.z = i.z / reference.z;

				if (var.x > 0.008856)
					var.x = pow(var.x, (1 / 3));
				else
					var.x = (7.787 * var.x) + (16.f / 116.f);

				if (var.y > 0.008856)
					var.y = pow(var.y, (1 / 3));
				else
					var.y = (7.787 * var.y) + (16.f / 116.f);

				if (var.z > 0.008856)
					var.z = pow(var.z, (1 / 3));
				else
					var.z = (7.787 * var.z) + (16.f / 116.f);

				float3 lab;

				lab.r = (116. * var.y) - 16.;
				lab.g = 500. * (var.x - var.y);
				lab.b = 200. * (var.y - var.z);

				return lab;*/

			}

			float3 RGBtoLAB(in float3 i)
			{
				return XYZtoLAB(RGBtoXYZ(i));
			}

			float Distance(in float3 c1, in float3 c2)
			{
				c1 = RGBtoLAB(c1);
				c2 = RGBtoLAB(c2);
				float distR = c1[0] - c2[0];
				float distG = c1[1] - c2[1];
				float distB = c1[2] - c2[2];
				return distR * distR + distG * distG + distB * distB;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				int x = 0;
				int choice = 0;
				int maxChoice = -1;
				float minDist = FLT_MAX;
				float maxDist = 0;
				for (x = 0; x < _MaxColors; x++)
				{
					float dist = Distance(_Colors[x], col);

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
                return _Colors[choice];
            }

			
            ENDCG
        }
    }
}
