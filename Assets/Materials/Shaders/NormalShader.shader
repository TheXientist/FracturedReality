Shader "Custom/NormalShader"
{
    SubShader
    {
        Pass
        {
            Tags {
                "RenderType" = "Opaque"
            }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // enable GPU instancing

            // Needed for UNITY_VERTEX_INPUT_INSTANCE_ID
            // appears to include everything commented out below
            #include "UnityCG.cginc"
            /*float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;*/

            struct VS_IN
            {
                float4 positionOS   : POSITION; //objectspace
                float3 normals : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FS_IN
            {
                float4 positionCS : SV_POSITION; //cameraspace
                float4 worldPos : WORLDPOS;
                float3 normals : FSNORMALS;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            FS_IN vert(VS_IN IN)
            {
                FS_IN OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(FS_IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                OUT.worldPos = worldPos;
                OUT.normals = IN.normals;
                return OUT;
            }

            float4 frag(FS_IN IN) : SV_TARGET
            {
                float4 clip_pos = mul(unity_MatrixVP, float4(IN.worldPos.x, IN.worldPos.y, IN.worldPos.z, 1.0));
                return float4(IN.normals.x, IN.normals.y, IN.normals.z, 1);
            }
            ENDHLSL
        }
    }
}