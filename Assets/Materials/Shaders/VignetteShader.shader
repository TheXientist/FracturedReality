Shader "Custom/VignetteShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Radius ("Radius", float) = 0
        _Feather ("Feather", float) = 0.8
        _TintColor ("Tint Color", Color) = (1,0,0,1)
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius, _Feather;
            float4 _TintColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 newUV = i.uv * 2 - 1;
                float circle = length(newUV);
                float mask = 1 - smoothstep(_Radius, _Radius + _Feather, circle);
                float invertMask = 1 - mask;

                float3 originalColor = col.rgb * mask;
                float3 vingetteColor = (1 - col.rgb) * _TintColor * invertMask;
                return fixed4(originalColor + vingetteColor, 1);
            }
            ENDCG
        }
    }
}
