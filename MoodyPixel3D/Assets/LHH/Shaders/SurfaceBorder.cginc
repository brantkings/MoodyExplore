fixed4 _BorderColor;
fixed _BorderThickness;

struct Input {
    float2 uv_MainTex;
};

void borderVert (inout appdata_full v) {
    v.vertex.xyz += v.normal * _BorderThickness;
}
void borderSurf (Input IN, inout SurfaceOutput o) {
    o.Emission = _BorderColor;
    o.Albedo = _BorderColor;
}
