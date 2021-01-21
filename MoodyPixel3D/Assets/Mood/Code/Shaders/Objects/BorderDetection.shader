Shader "Long Hat House/Unlit/BorderDetection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderColor ("Border Color", Color) = (0.5,1,1)
        _Limit ("Limit", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 depth : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BorderColor;
            fixed _Limit;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                //o.depth = float2(-1,-1);
                UNITY_TRANSFER_DEPTH(o.depth);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 derivative = abs(ddx(i.normal));
                
                
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
                //return derivative.xyzz * 10000000.0 / i.vertex.w;
                //UNITY_OUTPUT_DEPTH(i.depth);
                //return fixed4(i.depth.xy / 2, 0,0);
                //float multiplier = 100;
                //return fixed4(derivativeX * multiplier, derivativeY * multiplier, derivativeZ * multiplier, 1);
               //if(derivativeX * derivativeY  > _Limit * _Limit) return _BorderColor;
                //else return col;
            }
            ENDCG
        }
    }
}
