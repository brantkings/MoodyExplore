Shader "Custom/ScreenEffect/PixelArtCamera"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ResolutionX ("ResolutionX", float) = 128
		_ResolutionY ("ResolutionY", float) = 128
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
			
			#include "UnityCG.cginc"

			struct inputData
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(inputData v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			uniform fixed4 _Colors[256];
			float _ResolutionX;
			float _ResolutionY;
			sampler2D _RenderTex;

			fixed4 frag (v2f IN) : SV_Target
			{
				fixed4 original = tex2D(_MainTex, IN.uv);
				float2 randPos = frac(IN.uv * (183467) + _Time.y * 123);
				fixed4 col;
				fixed dist = 100000000.0;

				for (int i = 0; i < 16; i++) {
					fixed4 palette = _Colors[i];
					fixed d = distance(original, palette);

					if (d < dist) {
						dist = d;
						col = palette;
					}
				}
				return col;
			}
			ENDCG
		}
	}
}
