Shader "Custom/SineWave"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpeedMultiplier("Speed Multiplier", float) = 0
		_AmplitudeMultiplier("Amplitude Multiplier", float) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex fuck
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct fuckman
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

			v2f fuck(fuckman v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _SpeedMultiplier;
			float _AmplitudeMultiplier;

			fixed4 frag (v2f IN) : SV_Target
			{
				float sine = sin(IN.uv.x * 16 + _Time[1] * _SpeedMultiplier) - 0.5f;
				fixed4 col = tex2D(_MainTex, IN.uv + fixed2(0,sine * _AmplitudeMultiplier));
				//fixed4 col = lerp(texCol, fixed4(sine * IN.uv.x, cos(sine * IN.uv.y), 0.5, 0.5), sine);
				//clip(col - (IN.uv.x + IN.uv.y));
				return col;
			}
			ENDCG
		}
	}
}
