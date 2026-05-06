Shader "Custom/VoxelLit_PCSS_Stable_ForwardPlus"
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
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM

            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag

            // Main light shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // Forward+
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _Cutoff;
            float _ShadowSoftness;

            static const float2 poisson[12] =
            {
                float2(-0.326, -0.406),
                float2(-0.840, -0.074),
                float2(-0.696,  0.457),
                float2(-0.203,  0.621),
                float2( 0.962, -0.195),
                float2( 0.473, -0.480),
                float2( 0.519,  0.767),
                float2( 0.185, -0.893),
                float2( 0.507,  0.064),
                float2( 0.896,  0.412),
                float2(-0.322, -0.933),
                float2(-0.792, -0.598)
            };

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;

                return o;
            }

            float SampleSoftShadow(float3 positionWS)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);

                float baseShadow = MainLightRealtimeShadow(shadowCoord);

                float radius = _ShadowSoftness * shadowCoord.z;

                float accum = 0.0;

                [unroll]
                for (int i = 0; i < 12; i++)
                {
                    float4 coord = shadowCoord;
                    coord.xy += poisson[i] * radius;

                    accum += MainLightRealtimeShadow(coord);
                }

                float softShadow = accum / 12.0;

                return lerp(baseShadow, softShadow, 0.8);
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

                clip(tex.a - _Cutoff);

                float3 normalWS = normalize(i.normalWS);
                float3 albedo = tex.rgb;

                float3 color = 0;

                // Main light
                Light mainLight = GetMainLight();

                float shadow = SampleSoftShadow(i.positionWS);

                float NdotL = saturate(dot(normalWS, mainLight.direction));

                color += albedo *
                         mainLight.color *
                         NdotL *
                         shadow;

                // Forward+ additional lights
                #if defined(_ADDITIONAL_LIGHTS)

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);

                uint pixelLightCount = GetAdditionalLightsCount();

                LIGHT_LOOP_BEGIN(pixelLightCount)

                    Light light = GetAdditionalLight(lightIndex, i.positionWS);

                    float ndotl = saturate(dot(normalWS, light.direction));

                    color += albedo *
                             light.color *
                             ndotl *
                             light.distanceAttenuation *
                             light.shadowAttenuation;

                LIGHT_LOOP_END

                #endif

                float3 ambient = SampleSH(normalWS) * albedo;

                return half4(color + ambient, 1);
            }

            ENDHLSL
        }

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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);

                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = v.uv;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).a;

                clip(alpha - _Cutoff);

                return 0;
            }

            ENDHLSL
        }
    }
}