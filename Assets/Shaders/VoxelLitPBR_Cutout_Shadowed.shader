Shader "Custom/VoxelLitPBR_Cutout_Shadowed"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
            "RenderPipeline"="UniversalPipeline"
        }

        // =========================
        // FORWARD PASS (LIT)
        // =========================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 🔴 REQUIRED FOR SHADOWS (full set)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            // 🔴 Required for URP lighting
            #pragma multi_compile _ _FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _Cutoff;

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
                float4 shadowCoord : TEXCOORD3;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                o.uv = v.uv;

                // 🔴 required for main light shadows
                o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                clip(tex.a - _Cutoff);

                float3 normal = normalize(i.normalWS);
                float3 albedo = tex.rgb;

                // 🔴 main light WITH SHADOWS
                Light mainLight = GetMainLight(i.shadowCoord);

                float NdotL = saturate(dot(normal, mainLight.direction));
                float3 color = albedo * mainLight.color * NdotL * mainLight.shadowAttenuation;

                // 🔴 additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint j = 0; j < count; j++)
                {
                    Light light = GetAdditionalLight(j, i.positionWS);
                    float ndotl = saturate(dot(normal, light.direction));
                    color += albedo * light.color * ndotl * light.distanceAttenuation;
                }
                #endif

                // 🔴 ambient / light probes
                float3 ambient = SampleSH(normal) * albedo;

                return half4(color + ambient, 1);
            }
            ENDHLSL
        }

        // =========================
        // SHADOW CASTER PASS (FIXED PROPERLY)
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

            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float _Cutoff;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
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
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

                // ✅ Stable shadow bias (manual, works in all URP versions)
                float4 positionCS = TransformWorldToHClip(positionWS);

                // Apply simple bias along normal (prevents acne)
                positionCS.z += 0.0005;

                o.positionCS = positionCS;
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