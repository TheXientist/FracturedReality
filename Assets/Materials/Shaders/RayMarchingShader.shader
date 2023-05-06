Shader "Custom/RayMarching"
{
    SubShader
    {
        Tags{
            "Queue" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_CameraToWorld;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;
            float4 _ZBufferParams;
            float4 _ScreenParams;
            sampler2D _CameraDepthTexture;

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
                float2 screenPos : TEXCOORD1;
            };

            FS_IN vert(VS_IN IN)
            {
                FS_IN o;
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                o.positionCS = mul(unity_MatrixVP, worldPos);
                o.fragPosWS = worldPos;
                o.normals = IN.normals;
                o.screenPos = float2(o.positionCS.x * 0.5 + 0.5, o.positionCS.y * -0.5 + 0.5);
                return o;
            }

            float DEcube(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                return length(max(abs(pos) - 1, 0));
            }

            float DEsphere(float3 pos, float3 offset, float3 scale) {
                pos *= 1 / scale;
                pos -= offset;
                return length(-pos) - 1;
            }

            float DEisphere(float3 pos, float3 offset, float3 scale)
            {
                pos *= 1 / scale;
                pos -= offset;
                pos.xy = -(pos.xy % 1) - float3(0.5, 0.5, 0.5);
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

            float Mirror(float3 pos, float3 offset, float3 scale) {
                float3 mirrorPos = (-0.5, -1, -2);
                float3 mirrorNormal = (1, 1, 1);
                return min(length(-pos / scale + offset) - 0.5, length(-pos / scale + offset) - 0.5);
            }

            float DEplane(float3 pos, float3 offset, float3 scale) {
                pos = pos / scale - offset;
                float3 orientation = float3(1, 1, 1);
                return 1;
            }

            float DE(float3 pos) {
                return DEfractal1(pos, float3(0, 0, 0), float3(1, 1, 1));
            }

            float4 frag(FS_IN IN) : SV_TARGET
            {
                float3 view = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)); //camera view vector

                float cameraDepth = tex2D(_CameraDepthTexture, IN.screenPos).x; //camera depth texture
                float linearDepth = 1.0 / (_ZBufferParams.x * cameraDepth + _ZBufferParams.y); //linear camera depth
                float worldDepth = linearDepth * _ProjectionParams.z; //depth in world space

                float3 direction = normalize(IN.fragPosWS - _WorldSpaceCameraPos);
                float3 currentPos = _WorldSpaceCameraPos; //current position of the marching ray
                float3 lastPos = currentPos;

                float fidelity;
                float minFidelity = 0.001; //max dynamic fidelity

                float distance;

                int steps = 0;
                int maxSteps = 200;

                while (true) {
                    distance = DE(currentPos);
                    lastPos = currentPos;
                    currentPos += direction * distance;
                    ++steps;

                    if (dot(currentPos - _WorldSpaceCameraPos, view) > worldDepth) return float4(0, 0, 0, 0); //return miss if depth is higher than normal geometry (=occluded)
                    fidelity = max(sqrt(length(currentPos - _WorldSpaceCameraPos)) / 1000, minFidelity);
                    if (distance < fidelity) { //break if fidelity is reached

                        currentPos += direction * distance * (distance / fidelity) * (1 - dot(view, direction)) * 5; //extrapolate against depth banding
                        break;
                    }
                    if (steps >= maxSteps) break; //break if max steps are reached
                    if (length(currentPos - _WorldSpaceCameraPos) > _ProjectionParams.b) return float4(0, 0, 0, 0); //return miss if far clipping plane is reached
                }

                float ambientOcclusion = 1 - ((float)steps / (float)maxSteps);
                float depth = length(currentPos - _WorldSpaceCameraPos);

                //normals
                float3 normal;

                if (false) {
                    //fast ddx/ddy normals
                    normal = -normalize(cross(ddx(currentPos), ddy(currentPos)));
                } else {
                    //fancy normals
                    float3 dir = normalize(IN.fragPosWS + ddx(IN.fragPosWS) - _WorldSpaceCameraPos);
                    float3 pos = _WorldSpaceCameraPos;
                    int nSteps = 0;

                    while (nSteps++ < steps + 3) {
                        float dist = DE(pos);
                        pos += dir * dist;
                        if (dist < fidelity) {
                            pos += dir * dist * (dist / fidelity) * (1 - dot(view, dir)) * 5; //extrapolate against depth banding
                            break;
                        }
                    }
                    float3 nT = pos - currentPos;

                    dir = normalize(IN.fragPosWS + ddy(IN.fragPosWS) - _WorldSpaceCameraPos);
                    pos = _WorldSpaceCameraPos;
                    nSteps = 0;

                    while (nSteps++ < steps + 3) {
                        float dist = DE(pos);
                        pos += dir * dist;
                        if (dist < fidelity) {
                            pos += dir * dist * (dist / fidelity) * (1 - dot(view, dir)) * 5; //extrapolate against depth banding
                            break;
                        }
                    }
                    float3 nB = pos - currentPos;

                    normal = -normalize(cross(nT, nB));
                }

                //shadow ray
                float3 lightPos = float3(3, 10, 5);
                float intensity = 7;
                float exposure = 3;
                float3 lightDir = normalize(currentPos - lightPos);
                float3 currentLightPos = lightPos; //current position of the marching ray

                float LFidelity = fidelity * 2;
                int lightSteps = 0;
                int maxLightSteps = 50; //max amount of steps

                float minRatio = 1;
                float illumination = 0;

                while (true) {
                    distance = DE(currentLightPos);
                    ++lightSteps;
                    float ratio = distance / length(currentLightPos - currentPos);
                    currentLightPos += lightDir * distance;

                    if (ratio < minRatio) minRatio = ratio;

                    if (length(currentLightPos - currentPos) < LFidelity * 10) break; //break if the target location is within fidelity range
                    if (lightSteps >= maxLightSteps) break; //if something is hit or max steps are reached
                }

                if (length(currentLightPos - currentPos) < LFidelity * 10) {
                    float b = 1 /minRatio; //hypotenuse
                    float c = sqrt(b * b - 1); //ankathete = sqrt(b^2 - a^2)

                    float alpha = acos(c / b); 

                    float d = length(lightPos - currentPos); //distance from light source

                    float i = intensity;

                    for (int j = 1; j < exposure; ++j) {
                        i *= intensity;
                    }

                    illumination = (alpha / 1.57); //1.57 = PI/2
                }

                //specular
                float specularStrength = 10;
                float shininess = 32;
                float specular = pow(max(dot(view, reflect(-lightDir, normal)), 0.0), shininess) * illumination * specularStrength;

                float ambient = 0.04;
                float ambientIllumination = ambientOcclusion * ambient;
                float phong = dot(-lightDir, normal);

                float light = max(illumination + specular, ambientIllumination);

                //return float4(phong, phong, phong, 1);
                
                //return float4(specular, specular, specular, 1);
                //return float4(normal.x, normal.y, normal.z, 1);
                //return float4(illumination, illumination, illumination, 1);
                return float4(light, light, light, 1);
                //return  float4(ambientOcclusion, ambientOcclusion, ambientOcclusion, 1);
                //return  float4(steps % 2 * 0.5, 0, 0, 1); //distinguish marching layers
                //return  float4(lightSteps % 2 * 0.5, 0, 0, 1); //distinguish light layers
            }
            ENDHLSL
        }
    }
}