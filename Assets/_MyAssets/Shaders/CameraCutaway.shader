Shader "CorrentesDaNoite/CameraCutaway"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0
        _MetallicGlossMap("Metallic Map", 2D) = "white" {}
        _BumpScale("Normal Scale", Float) = 1
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _OcclusionStrength("Occlusion", Range(0,1)) = 1
        _OcclusionMap("Occlusion Map", 2D) = "white" {}
        [HDR] _EmissionColor("Emission", Color) = (0,0,0,1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        [HideInInspector] _SpecColor("Specular", Color) = (0.2,0.2,0.2,1)
        [HideInInspector] _SpecGlossMap("Specular Map", 2D) = "white" {}
        [HideInInspector] _Surface("Surface", Float) = 0
        [HideInInspector] _Cull("Cull", Float) = 2
        [HideInInspector] _OcclusionAmount("Cutaway", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "UniversalMaterialType" = "Lit" }
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

        #define CUT_SOFTNESS 0.25
        #define CUT_CLEARANCE 0.25
        #define WOBBLE_AMOUNT 0.06
        #define WOBBLE_WAVES 5
        #define WOBBLE_SPEED 2.5
        #define PULSE_AMOUNT 0.04
        float _CameraOcclusionActive;
        float4 _CameraOcclusionTarget;
        float _CameraOcclusionFloor;
        float _OcclusionAmount;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float2 uv : TEXCOORD0;
            float2 lightmapUV : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half4 tangentWS : TEXCOORD2;
            float2 uv : TEXCOORD3;
            DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
            half fog : TEXCOORD5;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings Vertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
            VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS, input.tangentOS);
            output.positionCS = position.positionCS;
            output.positionWS = position.positionWS;
            output.normalWS = normal.normalWS;
            output.tangentWS = half4(normal.tangentWS, input.tangentOS.w * GetOddNegativeScale());
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
            OUTPUT_SH(output.normalWS, output.vertexSH);
            output.fog = ComputeFogFactor(position.positionCS.z);
            return output;
        }

        float Bayer(float2 pixel)
        {
            static const float pattern[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
            uint2 p = uint2(pixel) % 4;
            return (pattern[p.y * 4 + p.x] + 0.5) / 16.0;
        }

        void Cutaway(Varyings input)
        {
            if (_CameraOcclusionActive < 0.5 || _OcclusionAmount <= 0 || input.positionWS.y < _CameraOcclusionFloor) return;
            float3 relative = input.positionWS - _WorldSpaceCameraPos;
            float3 toTarget = _CameraOcclusionTarget.xyz - _WorldSpaceCameraPos;
            // compara no chao pq a camera olha de cima e o alto da parede do fundo parece estar na frente
            float3 forward = -UNITY_MATRIX_V[2].xyz;
            float2 flat = normalize(forward.xz + float2(0.0001, 0));
            if (dot(relative.xz, flat) > dot(toTarget.xz, flat) - CUT_CLEARANCE) return;
            float lengthToTarget = max(length(toTarget), 0.001);
            float3 direction = toTarget / lengthToTarget;
            float along = dot(relative, direction);
            if (along < 0) return;
            // so fura se o pixel ta mesmo na frente do alvo olhando por ele e nao do lado
            float fragmentDistance = length(relative);
            if (fragmentDistance > dot(toTarget, relative / max(fragmentDistance, 0.001)) - CUT_CLEARANCE) return;
            float3 offset = relative - direction * along;
            float3 up = abs(direction.y) < 0.99 ? float3(0, 1, 0) : float3(1, 0, 0);
            float3 right = normalize(cross(up, direction));
            float angle = atan2(dot(offset, cross(direction, right)), dot(offset, right));
            float time = _Time.y * WOBBLE_SPEED;
            float wave = sin(angle * WOBBLE_WAVES + time) * 0.6 + sin(angle * (WOBBLE_WAVES + 2) - time * 1.3) * 0.4;
            float pulse = 1 + sin(time * 1.7) * PULSE_AMOUNT;
            float radius = _CameraOcclusionTarget.w * along / lengthToTarget * _OcclusionAmount * pulse * (1 + wave * WOBBLE_AMOUNT);
            float cut = (1 - smoothstep(radius * (1 - CUT_SOFTNESS), radius, length(offset))) * saturate(_OcclusionAmount * 4);
            clip(Bayer(input.positionCS.xy) - cut);
        }

        half3 Normal(Varyings input, half3 normalTS)
        {
            half3 normalWS = normalize(input.normalWS);
            #if defined(_NORMALMAP)
            half3 tangent = normalize(input.tangentWS.xyz);
            half3 bitangent = cross(normalWS, tangent) * input.tangentWS.w;
            normalWS = TransformTangentToWorld(normalTS, half3x3(tangent, bitangent, normalWS));
            #endif
            return normalize(normalWS);
        }

        half4 Fragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            Cutaway(input);
            SurfaceData surface;
            InitializeStandardLitSurfaceData(input.uv, surface);
            InputData data = (InputData)0;
            data.positionWS = input.positionWS;
            data.normalWS = Normal(input, surface.normalTS);
            data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
            data.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            data.fogCoord = input.fog;
            data.vertexLighting = VertexLighting(input.positionWS, data.normalWS);
            data.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, data.normalWS);
            data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
            data.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
            half4 color = UniversalFragmentPBR(data, surface);
            color.rgb = MixFog(color.rgb, input.fog);
            return color;
        }

        half4 DepthFragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            Cutaway(input);
            Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
            return 0;
        }

        half4 NormalsFragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            Cutaway(input);
            SurfaceData surface;
            InitializeStandardLitSurfaceData(input.uv, surface);
            half3 normalWS = Normal(input, surface.normalTS);
            #if defined(_GBUFFER_NORMALS_OCT)
            float2 oct = PackNormalOctQuadEncode(normalWS);
            return half4(PackFloat2To888(saturate(oct * 0.5 + 0.5)), 0);
            #else
            return half4(normalWS, 0);
            #endif
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment DepthFragment
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vertex
            #pragma fragment NormalsFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
}