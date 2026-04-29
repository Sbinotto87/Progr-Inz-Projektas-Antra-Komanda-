Shader "Custom/VoxelLit_PCSS_Stable"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5

        _ShadowSoftness ("Shadow Softness", Range(0.0005, 0.01)) = 0.003
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _Cutoff;
            float _ShadowSoftness;

            // =========================
            // POISSON DISK
            // =========================
            static const float2 poisson[12] =
            {
                float2(-0.326, -0.406), float2(-0.840, -0.074),
                float2(-0.696,  0.457), float2(-0.203,  0.621),
                float2( 0.962, -0.195), float2( 0.473, -0.480),
                float2( 0.519,  0.767), float2( 0.185, -0.893),
                float2( 0.507,  0.064), float2( 0.896,  0.412),
                float2(-0.322, -0.933), float2(-0.792, -0.598)
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                o.uv = v.uv;

                return o;
            }

            // =========================
            // STABLE PCSS-STYLE SHADOW
            // =========================
            float SampleSoftShadow(float3 positionWS, float3 normalWS)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);

                // Base shadow (URP PCF)
                float baseShadow = MainLightRealtimeShadow(shadowCoord);

                // Distance-based softness
                float depth = shadowCoord.z;
                float radius = _ShadowSoftness * depth;

                float accum = 0;

                // Poisson filtering (soft edges)
                for (int i = 0; i < 12; i++)
                {
                    float2 offset = poisson[i] * radius;

                    float4 offsetCoord = shadowCoord;
                    offsetCoord.xy += offset;

                    accum += MainLightRealtimeShadow(offsetCoord);
                }

                float softShadow = accum / 12.0;

                // Blend sharp + soft → PCSS-like
                return lerp(baseShadow, softShadow, 0.8);
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                clip(tex.a - _Cutoff);

                float3 normal = normalize(i.normalWS);
                float3 albedo = tex.rgb;

                Light mainLight = GetMainLight();

                float NdotL = saturate(dot(normal, mainLight.direction));

                float shadow = SampleSoftShadow(i.positionWS, normal);

                float3 color = albedo * mainLight.color * NdotL * shadow;

                // additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint j = 0; j < count; j++)
                {
                    Light light = GetAdditionalLight(j, i.positionWS);
                    float ndotl = saturate(dot(normal, light.direction));
                    color += albedo * light.color * ndotl * light.distanceAttenuation;
                }
                #endif

                float3 ambient = SampleSH(normal) * albedo;

                return half4(color + ambient, 1);
            }

            ENDHLSL
        }

        // =========================
        // SHADOW CASTER
        // =========================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float _Cutoff;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = v.uv;

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }

            ENDHLSL
        }
    }
}