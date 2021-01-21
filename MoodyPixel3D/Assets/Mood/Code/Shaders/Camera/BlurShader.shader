// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LongHatHouse/BlurShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Strength ("Strength", float) = 0 
		_FocalPoint ("Focal Point", float) = 0.1
		_Passes ("Passes", int) = 1
		_EachPassDecay ("EachPassDecay", float) = 0
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;

				/*v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.vertex =  mul(UNITY_MATRIX_MVP, v.vertex);
				//o.scrPos = ComputeScreenPos(o.vertex);
				//o.scrPos = float4(0,0,0,0); //TODO: Change this
				o.uv = v.uv;
				return o;*/
			}
			
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _Strength;
			float _FocalPoint;
			int _Passes;
			float _EachPassDecay;

			float GetDepthValue(float2 uv)
			{
				return Linear01Depth(tex2D(_CameraDepthTexture, uv).r);
			}

			fixed4 frag (v2f input) : SV_Target
			{
				float depthValue = GetDepthValue(input.uv);
				fixed4 col = tex2D(_MainTex, input.uv);

				float2 up = float2(0, _MainTex_TexelSize.y);
				float2 right = float2(_MainTex_TexelSize.x, 0);

				fixed4 addedColor = col;
				half outsideColorForce = 1;
				for(int i=0; i<_Passes; i++)
				{
					fixed4 colU = tex2D(_MainTex, input.uv + up * i);
					fixed4 colD = tex2D(_MainTex, input.uv - up * i);
					fixed4 colR = tex2D(_MainTex, input.uv - right * i);
					fixed4 colL = tex2D(_MainTex, input.uv + right * i);
					
					fixed4 outsideColor = (colU + colD + colR + colL) * 0.25;
					addedColor = addedColor * (1-outsideColorForce) + outsideColorForce * outsideColor;
					outsideColorForce -= _EachPassDecay;
				}


				float focalPointDistance = abs(_FocalPoint - depthValue);
				float blurStrength = _Strength * (1-focalPointDistance);

				col = (1 - blurStrength ) * col + blurStrength * addedColor;

				return col;
			}
			ENDCG
		}
	}
}
