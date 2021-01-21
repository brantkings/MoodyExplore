Shader "LongHatHouse/TriplanarUnlit"
{
    Properties
    {
		_GroundTex("Texture", 2D) = "white" {}
		_WallTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float3 localNormal : TEXCOORD1;
				float3 worldPos: TEXCOORD2;
                float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(3)
            };

			sampler2D _GroundTex;
			sampler2D _WallTex;
            float4 _GroundTex_ST;
			float4 _WallTex_ST;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.localNormal = UnityObjectToWorldNormal(v.normal);
				UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // triplanar shader but only use xz, so its like an uniplanar shader.
				fixed4 up = tex2D(_GroundTex, i.worldPos.xz / _GroundTex_ST.xy);
				fixed4 right = tex2D(_WallTex, i.worldPos.zy / _WallTex_ST.xy);
				fixed4 forward = tex2D(_WallTex, i.worldPos.xy / _WallTex_ST.xy);
                // apply fog
				fixed4 col = (up * abs(i.localNormal.y) + right * abs(i.localNormal.x) + forward * abs(i.localNormal.z)) / length(i.localNormal);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
