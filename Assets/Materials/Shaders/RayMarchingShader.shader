// This defines a simple unlit Shader object that is compatible with a custom Scriptable Render Pipeline.
// It applies a hardcoded color, and demonstrates the use of the LightMode Pass tag.
// It is not compatible with SRP Batcher.

Shader "Examples/RayMarching"
{
    SubShader
    {
        Pass
        {
        Tags {
            "Queue" = "Background+1"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "LightMode" = "ExampleLightModeTag"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;
            sampler2D _LastCameraDepthTexture;

            struct VS_IN
            {
                float4 positionOS   : POSITION; //objectspace
                float3 normals : NORMAL;
            };

            struct FS_IN
            {
                float4 positionCS : SV_POSITION; //cameraspace
                float4 fragPosWS : WORLDPOS;
                float3 normals : FSNORMALS;
                float4 screenPos : TEXCOORD1;
            };

            struct FS_OUT
            {
                float4 color;
                float zvalue;
            };

            FS_IN vert(VS_IN IN)
            {
                FS_IN OUT;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.positionCS = mul(unity_MatrixVP, worldPos);
                OUT.fragPosWS = worldPos;
                OUT.normals = IN.normals;
                return OUT;
            }

            float DEsphere(float3 currentPos, float3 offset) {
                return length(offset - currentPos) - 1;
            }

            float DEcube(float3 currentPos, float3 offset) {
                return length(max(abs(offset - currentPos) - 1, 0));
            }

            float DEisphere(float3 z, float3 offset)
            {
                z.xy = offset - (z.xy % 1) - float3(0.5, 0.5, 0.5);
                return length(z) - 0.3;
            }

            float DEtetra(float3 z) {
                int Iterations = 10;
                float Scale = 2.0;
                float3 a1 = float3(1, 1, 1);
                float3 a2 = float3(-1, -1, 1);
                float3 a3 = float3(1, -1, -1);
                float3 a4 = float3(-1, 1, -1);
                float3 c;
                int n = 0;
                float dist, d;
                while (n < Iterations) {
                    c = a1; dist = length(z - a1);
                    d = length(z - a2); if (d < dist) { c = a2; dist = d; }
                    d = length(z - a3); if (d < dist) { c = a3; dist = d; }
                    d = length(z - a4); if (d < dist) { c = a4; dist = d; }
                    z = Scale * z - c * (Scale - 1.0);
                    n++;
                }

                return length(z) * pow(Scale, float(-n));
            }

            float mandelbulb(float3 pos, float3 offset) { //mandelbrot
                pos *= 1/1; //replacesecond value for scaling
                pos -= offset;
                float Bailout = 4;
                int Iterations = 20;
                int Power = 2;
                float3 z = pos;
                float dr = 1.0;
                float r = 0.0;
                for (int i = 0; i < Iterations; i++) {
                    r = length(z);
                    if (r > Bailout) break;

                    // convert to polar coordinates
                    float theta = acos(z.z / r);
                    float phi = atan2(z.y, z.x);
                    dr = pow(r, Power - 1.0) * Power * dr + 1.0;

                    // scale and rotate the point
                    float zr = pow(r, Power);
                    theta = theta * Power;
                    phi = phi * Power;

                    // convert back to cartesian coordinates
                    z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
                    z += pos;
                }
                return 0.5 * log(r) * r / dr;
                return 0.5 / length(pos - offset) * log(r) * r / dr; //specifies how much of the result should be returned, lower value undershoots less and increases distance where bulb disappears
            }

            float DE(float3 pos) {
                return DEisphere(pos, float3(0, 0, 0));
            }
            
            FS_OUT frag(FS_IN IN) : SV_TARGET
            {
                FS_OUT o;

                float a = 50; //inverse ambient occlusion strength
                float3 direction = normalize(IN.fragPosWS - _WorldSpaceCameraPos);
                float3 currentPos = _WorldSpaceCameraPos; //current position of the marching ray
                float maxFidelity = 0.0005; //max dynamic fidelity
                float fidelity = max(DE(_WorldSpaceCameraPos) / 1000, maxFidelity); //fidelity increases dynamically with distance from target, automatic LOD
                float lastDistance = fidelity + 1; //upcoming marching step
                float thisDistance = 0;
                float smallestDistance = _ProjectionParams.b; //smallest recorded distance, necessary for glow

                int steps = 0;
                int maxSteps = 10 / fidelity; //max amount of steps
                maxSteps = 200;

                while (length(currentPos - _WorldSpaceCameraPos) < _ProjectionParams.b) {
                    lastDistance = thisDistance;
                    thisDistance = DE(currentPos);
                    if (smallestDistance > thisDistance) {
                        smallestDistance = thisDistance;
                    }
                    currentPos += direction * thisDistance;

                    if (thisDistance < fidelity) break; //break if fidelity is reached
                    if (++steps >= maxSteps) break; //break if max steps are reached
                    if (length(currentPos - _WorldSpaceCameraPos) > _ProjectionParams.b) break; //break if far clipping plane is reached
                }

                if (length(currentPos - _WorldSpaceCameraPos) >= _ProjectionParams.b) {
                    //float glow = 1 / smallestDistance * 0.1;
                    o.zvalue = 1;
                    o.color = float4(0, 0, 0, 0); //returns a black pixel if the ray reached the far clipping plane
                    return o;
                }

                //float3 xDir = float3(1, 0, 0);
                //float3 yDir = float3(0, 1, 0);
                //float3 zDir = float3(0, 0, 1);

                //semi functioning normals
                /*
                float3 nA = normalize(float3(DE(currentPos + xDir) - DE(currentPos - xDir), 
                    DE(currentPos + yDir) - DE(currentPos - yDir),
                    DE(currentPos + zDir) - DE(currentPos - zDir)));
                */

                //dfdx normals
                //float3 nB = ddx(currentPos * 100);

                float ambientOcclusion = 1 - ((float)steps / (a + (float)steps));


                float4 clip_pos = mul(unity_MatrixVP, float4(currentPos, 1.0));
                float depth = clip_pos.z / clip_pos.w;

                float rayLength = length(currentPos - _WorldSpaceCameraPos); //how far did the ray march before hitting its target?

                o.zvalue = depth;
                o.color = float4(ambientOcclusion - (1 - depth) * 0.1, ambientOcclusion - (1 - depth) * 0.05, ambientOcclusion, 1);
                o.color = float4(depth, depth, depth, 1);
                return o;
            }
            ENDHLSL
        }
    }
}