// This defines a simple unlit Shader object that is compatible with a custom Scriptable Render Pipeline.
// It applies a hardcoded color, and demonstrates the use of the LightMode Pass tag.
// It is not compatible with SRP Batcher.

Shader "Examples/NormalShader"
{
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "ExampleLightModeTag"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;

            struct VS_IN
            {
                float4 positionOS   : POSITION; //objectspace
                float3 normals : NORMAL;

            };

            struct FS_IN
            {
                float4 positionCS : SV_POSITION; //cameraspace
                float4 worldPos : WORLDPOS;
                float3 normals : FSNORMALS;
            };

            FS_IN vert(VS_IN IN)
            {
                FS_IN OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                OUT.worldPos = worldPos;
                OUT.normals = IN.normals;
                return OUT;
            }

            struct FS_OUT
            {
                float4 color;
                float zvalue;
            };

            FS_OUT frag(FS_IN IN) : SV_TARGET
            {
                FS_OUT o;
                float4 clip_pos = mul(unity_MatrixVP, float4(IN.worldPos.x, IN.worldPos.y, IN.worldPos.z, 1.0));
                o.zvalue = clip_pos.z / clip_pos.w;
                o.color = float4(IN.normals.x, IN.normals.y, IN.normals.z, 1);
                return o;
            }
            ENDHLSL
        }
    }
}