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
            "LightMode" = "RayMarching"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Off

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
                float4 fragPosWS : WORLDPOS;
                float4 lightPosWS : LIGHTPOS;
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
                FS_IN o;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                o.positionCS = mul(unity_MatrixVP, worldPos);
                o.fragPosWS = worldPos;
                o.normals = IN.normals;
                return o;
            }

            float DEcube(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                return length(max(abs(float3(0, 0, 0) - pos) - 1, 0));
            }

            float DEsphere(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                return length(float3(0, 0, 0) - pos) - 1;
            }

            float DEisphere(float3 pos, float3 offset, float3 scale)
            {
                pos *= 1 / scale;
                pos -= offset;
                pos.xy = float3(0, 0, 0) - (pos.xy % 1) - float3(0.5, 0.5, 0.5);
                return length(pos) - 0.3;
            }

            float DEtorus(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                float radius = 0.5;
                float inner = 0.2;
                return length(float2(length(pos.xz) - radius, pos.y)) - inner;
            }

            float DEtetra(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                int Iterations = 10;
                float rescale = 2.0;
                float3 a1 = float3(1, 1, 1);
                float3 a2 = float3(-1, -1, 1);
                float3 a3 = float3(1, -1, -1);
                float3 a4 = float3(-1, 1, -1);
                float3 c;
                int n = 0;
                float dist, d;
                while (n < Iterations) {
                    c = a1; dist = length(pos - a1);
                    d = length(pos - a2); if (d < dist) { c = a2; dist = d; }
                    d = length(pos - a3); if (d < dist) { c = a3; dist = d; }
                    d = length(pos - a4); if (d < dist) { c = a4; dist = d; }
                    pos = rescale * pos - c * (rescale - 1.0);
                    n++;
                }

                return length(pos) * pow(rescale, float(-n));
            }

            float DEfractal1(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                float s = 3;
                pos = abs(pos);
                float3  p0 = pos * 0.9;
                for (float i = 0; i < 5; i++) {
                    pos = 1 - abs(pos - 1);
                    pos = 1 - abs(abs(pos - 2) - 1);
                    float g = -4.5 * clamp(0.45 * max(1.6 / dot(pos, pos), 0.7), 0, 1.2);
                    pos *= g;
                    pos += p0;
                    s *= g;
                }
                s = abs(s);
                float a = 3.8;
                pos -= clamp(pos, -a, a);
                return length(pos) / s;
            }

            float Mandelbulb(float3 pos, float3 offset, float3 scale) { //mandelbulb
                pos *= 1 / scale;
                pos -= offset;
                float Bailout = 4;
                int Iterations = 20;
                int Power = 8;
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
                return 0.5 / length(pos - offset) * log(r) * r / dr;
            }

            float Union(float3 pos, float3 offset, float3 scale) {
                return min(DEtorus(pos, offset, float3(2, 2, 2)), DEcube(pos, offset, scale));
            }

            float Intersection(float3 pos, float3 offset, float3 scale) {
                return max(DEtorus(pos, offset, float3(2, 2, 2)), DEcube(pos, offset, scale));
            }

            float Difference(float3 pos, float3 offset, float3 scale) {
                return max(-DEtorus(pos, offset, float3(2, 2, 2)), DEcube(pos, offset, scale));
            }

            float DE(float3 pos) {
                return Difference(pos, float3(0, 0, 0), float3(1, 1, 1));
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

                while (true) {
                    lastDistance = thisDistance;
                    thisDistance = DE(currentPos);
                    ++steps;
                    if (smallestDistance > thisDistance) {
                        smallestDistance = thisDistance;
                    }
                    currentPos += direction * thisDistance;

                    if (thisDistance < fidelity) break; //break if fidelity is reached
                    if (steps >= maxSteps) break; //break if max steps are reached
                    if (length(currentPos - _WorldSpaceCameraPos) > _ProjectionParams.b) break; //break if far clipping plane is reached
                }

                if (length(currentPos - _WorldSpaceCameraPos) >= _ProjectionParams.b) {
                    //float glow = 1 / smallestDistance * 0.1;
                    o.zvalue = 1;
                    o.color = float4(0, 0, 0, 0); //returns a transparent pixel if the ray reached the far clipping plane
                    return o;
                }

                //raymarched lighting
                float3 lightPos = float3(10, 10, 10);
                float intensity = 7;
                float exposure = 3;
                float3 lightDir = normalize(currentPos - lightPos);
                float3 currentLightPos = lightPos; //current position of the marching ray

                float lightFidelity = 0.001;
                int lightSteps = 0;
                int maxLightSteps = 50; //max amount of steps

                float minRatio = 1;
                float illumination = 0;

                while (true) {
                    float distance = DE(currentLightPos);
                    ++lightSteps;
                    float ratio = distance / length(currentLightPos - currentPos);
                    currentLightPos += lightDir * distance;

                    if (ratio < minRatio) minRatio = ratio;

                    if (length(currentLightPos - currentPos) < lightFidelity * 10) break; //break if the target location is within fidelity range
                    if (distance < lightFidelity) break; //break if fidelity is reached
                    if (lightSteps >= maxLightSteps) break; //break if max steps are reached
                }

                if (length(currentLightPos - currentPos) < lightFidelity * 10) { //if target was hit
                    float c = 1 / minRatio; //kathete, radius

                    float b = sqrt(c * c - 1); //hypotenuse

                    float d = length(lightPos - currentPos); //distance from light source

                    float i = intensity;

                    for (int j = 1; j < exposure; ++j) {
                        i *= intensity;
                    }

                    illumination = (1 - asin(b / c) / 1.57) / (d * d) * i;
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

                //illumination = lightSteps / 100.0;

                o.color = float4(illumination, illumination, illumination, 1);
                //o.color = float4(ambientOcclusion - (1 - depth) * 0.1, ambientOcclusion - (1 - depth) * 0.05, ambientOcclusion, 1);
                //o.color = float4(depth, depth, depth, 1);
                return o;
            }
            ENDHLSL
        }
    }
}