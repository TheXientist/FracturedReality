Shader "Custom/RayMarching"
{
    Properties{
        _FractalCount ("Fractal Count", Integer) = 0

        _VR ("VR", Integer) = 1

        _DepthSteps ("Depth Marching Steps", Integer) = 200
        _LightSteps ("Light Marching Steps", Integer) = 50
        _Fidelity ("Raymarching Detail", Float) = 0.001
        _AO ("Ambient Occlusion Strength", Float) = 5
        _IAmbient ("Ambient Brightness", FLoat) = 0.07
        _ISpecular ("Specular Intensity", Float) = 5
        _Shininess ("Shininess", Float) = 5
        _NormalMode ("Normal Rendering Mode", Int) = 1
        _Fov ("Foveated Rendering Curve", Float) = 2.4
        _FovSteps ("FR Minimum Marching Steps", Integer) = 0
    }
    SubShader
    {
        Tags{
            "Queue" = "Transparent-1"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // enable GPU instancing

            #include "UnityCG.cginc"
            
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);
            
            // Must be identical to the struct in Fractal.cs
            struct FractalData
            {
                int type;
                float4x4 worldToLocal;
            };
            StructuredBuffer<FractalData> _FractalBuffer;
            
            int _FractalCount;
            int _DepthSteps;
            int _LightSteps;
            float _Fidelity;
            int _VR;
            float _AO;
            float _IAmbient;
            float _ISpecular;
            float _Shininess;
            int _NormalMode;
            float _Fov;
            int _FovSteps;

            struct VS_IN
            {
                float4 positionOS   : POSITION; //objectspace
                float3 normals : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FS_IN
            {
                float4 positionCS : SV_POSITION; //cameraspace
                float4 fragPosWS : WORLDPOS;
                float4 lightPosWS : LIGHTPOS;
                float3 normals : FSNORMALS;
                float2 screenPos : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            FS_IN vert(VS_IN IN)
            {
                FS_IN o;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(FS_IN, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                float4 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                o.positionCS = mul(unity_MatrixVP, worldPos);
                o.fragPosWS = worldPos;
                o.normals = IN.normals;
                o.screenPos = float2(o.positionCS.x * 0.5 + 0.5, o.positionCS.y * -0.5 + 0.5);
                return o;
            }

            float DEcube(float3 pos) {
                return length(max(abs(pos) - 1, 0));
            }

            float DEsphere(float3 pos) {

                return length(-pos) - 1;
            }

            float DEisphere(float3 pos)
            {

                pos.xy = -(pos.xy % 1) - float3(0.5, 0.5, 0.5);
                return length(pos) - 0.3;
            }

            float DEtorus(float3 pos) {
                float radius = 0.5;
                float inner = 0.2;
                return length(float2(length(pos.xz) - radius, pos.y)) - inner;
            }

            float DEtetra(float3 pos) {

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

            float DEfractal(float3 pos) {
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

            float Mandelbulb(float3 pos) { //mandelbulb

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
                return 0.5 / length(pos) * log(r) * r / dr;
            }

            float Union(float3 pos) {
                return min(DEtorus(pos), DEcube(pos));
            }

            float Intersection(float3 pos) {
                return max(DEtorus(pos), DEcube(pos));
            }

            float Difference(float3 pos) {
                return max(-DEtorus(pos), DEcube(pos));
            }

            float DEplane(float3 pos) {
                float3 orientation = float3(1, 1, 1);
                return 1;
            }

            float DE(float3 pos) {
                float minDist = _ProjectionParams.z;
                float4 pos4 = float4(pos,1); // Needed for matrix transformation
                
                for (int i = 0; i < _FractalCount; i++)
                {
                    FractalData frac = _FractalBuffer[i];
                    float3 localPos = mul(frac.worldToLocal, pos4);
                    float dist = 0.0f;

                    switch (frac.type)
                    {
                    case 0:
                        dist = DEfractal(localPos);
                        break;
                    case 1:
                        dist = Mandelbulb(localPos);
                        break;
                    default:
                        dist = minDist;
                        break;
                    }

                    if (dist < minDist) minDist = dist;
                }
                return minDist;
            }
            
            float4 frag(FS_IN IN) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                
                float3 view = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)); //camera view vector

                if(_VR == 1) IN.screenPos.y = 1 - IN.screenPos.y; //Flip y in VR

                float cameraDepth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, IN.screenPos).x; //camera depth texture
                float linearDepth = 1.0 / (_ZBufferParams.x * cameraDepth + _ZBufferParams.y); //linear camera depth
                float worldDepth = linearDepth * _ProjectionParams.z; //depth in world space

                float3 direction = normalize(IN.fragPosWS - _WorldSpaceCameraPos);
                float3 currentPos = _WorldSpaceCameraPos; //current position of the marching ray
                float3 lastPos = currentPos;

                float fidelity;

                float distance;

                int steps = 0;

                //Foveated LOD
                float2 centerPos = float2(IN.screenPos.x * 2 - 1, IN.screenPos.y * 2 - 1);

                _DepthSteps = lerp(_DepthSteps, _FovSteps, pow(length(centerPos), _Fov));

                while (steps < _DepthSteps) {
                    distance = DE(currentPos);
                    lastPos = currentPos;
                    currentPos += direction * distance;
                    ++steps;

                    if (dot(currentPos - _WorldSpaceCameraPos, view) > worldDepth) return float4(0, 0, 0, 0); //return miss if depth is higher than normal geometry (=occluded)
                    fidelity = max(sqrt(length(currentPos - _WorldSpaceCameraPos)) / 1000, _Fidelity);
                    if (distance < fidelity) { //break if fidelity is reached

                        currentPos += direction * distance * (distance / fidelity) * (1 - dot(view, direction)) * 5; //extrapolate against depth banding
                        break;
                    }

                    if (length(currentPos - _WorldSpaceCameraPos) > _ProjectionParams.b) return float4(0, 0, 0, 0); //return miss if far clipping plane is reached
                }

                float ambientOcclusion = pow(1 - ((float)steps / (float)_DepthSteps), _AO);
                float depth = length(currentPos - _WorldSpaceCameraPos);

                //normals
                float3 normal;

                if (_NormalMode == 0) {
                    //fast ddx/ddy normals
                    normal = -normalize(cross(ddx(currentPos), ddy(currentPos)));
                } else if (_NormalMode == 1) {
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

                if (_VR) normal = -normal;

                //shadow ray
                float3 lightDir = -float3(_WorldSpaceLightPos0.x, _WorldSpaceLightPos0.y, _WorldSpaceLightPos0.z);
                float3 lightPos = currentPos - lightDir * _ProjectionParams.z;
                float intensity = 3;
                float exposure = 1;
                float3 currentLightPos = lightPos; //current position of the marching ray

                int lightSteps = 0;

                float minRatio = 1;

                float illumination = 0;

                bool hit = false;
                float insideDist = 0;

                while (true) {
                    distance = DE(currentLightPos);
                    ++lightSteps;
                    float ratio = distance / length(currentLightPos - currentPos);
                    currentLightPos += lightDir * distance;
                    if (ratio < minRatio) minRatio = ratio;

                    if (length(currentLightPos - currentPos) < fidelity * 8) break; //if the target location is within fidelity range

                    if (lightSteps >= _LightSteps || distance < fidelity) { //when max steps are reached
                        hit = true;
                        break;
                    }
                }

                float d = length(lightPos - currentPos); //distance from light source
                float i = pow(intensity, exposure);
                if (false) {
                    i = pow(intensity, exposure) / (d * d); //inverse square law
                }

                if (!hit) {
                    float b = 1 / minRatio; //hypotenuse
                    float c = sqrt(b * b - 1); //ankathete = sqrt(b^2 - a^2)

                    float alpha = acos(c / b);

                    illumination = min(dot(-lightDir, normal) * i, (alpha / 1.57) * i); //1.57 = PI/2
                }

                //specular
                float specular = pow(max(dot(normalize(currentPos - _WorldSpaceCameraPos), reflect(-lightDir, normal)), 0.0), pow(2, _Shininess)) * illumination * max(_ISpecular, 0);

                float ambientIllumination = ambientOcclusion * _IAmbient;

                float light = max(illumination, ambientIllumination);
                float3 baseColor = abs(normal);
                
                //return  float4(steps % 2 * 0.5, 0, 0, 1); //distinguish marching layers
                //return  float4(lightSteps % 2 * 0.5, 0, 0, 1); //distinguish light layers

                //return float4(specular, specular, specular, 1);
                //return float4(normal.x, normal.y, normal.z, 1);
                //return float4(baseColor.x, baseColor.y, baseColor.z, 1);
                //return float4(illumination, illumination, illumination, 1);
                //return float4(ambientOcclusion, ambientOcclusion, ambientOcclusion, 1);
                //return float4(light, light, light, 1);
                return float4(light * baseColor.x + specular, light * baseColor.y + specular, light * baseColor.z + specular, 1);
            }
            ENDHLSL
        }
    }
}