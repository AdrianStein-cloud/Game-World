Shader "HackerShader"
{
    Properties
    {
        _FontTex("Font Texture (16×16 atlas)", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D)        = "white" {}
        _TimeSpeedFont("Time Speed Font", Range(0.0, 0.01)) = 0.005
        _TimeSpeedRain("Time Speed Rain", Range(0.0, 25.0)) = 5.0
        _HackerMinDrawDistance("Min Draw Distance", Float) = 5.0
        _HackerMaxDrawDistance("Max Draw Distance", Float) = 10.0
        _HackerFadeRange("Fade Range", Float) = 2.5
        _HackerOverallAlpha("Overall Alpha", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "FullscreenEffectPass"
            
            // Render State
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha // MODIFIED: Enable alpha blending
            ZTest LEqual
            ZWrite Off
            
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols
                

            // Defines
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_VERTEXID
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1
            
        // Includes
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        struct Attributes
        {
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
             uint vertexID : VERTEXID_SEMANTIC;
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
             float4 texCoord1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            return output;
        }
            
        // --------------------------------------------------
        // Graph Properties CBuffer
        CBUFFER_START(UnityPerMaterial)
        CBUFFER_END
        
        // Screen blit texture (not used)
        float _FlipY;
        TEXTURE2D_X(_BlitTexture);
        
        // Our custom textures & samplers
        TEXTURE2D_X(_FontTex);   SAMPLER(sampler_FontTex);
        TEXTURE2D_X(_NoiseTex);  SAMPLER(sampler_NoiseTex);
        float _TimeSpeedFont;
        float _TimeSpeedRain;
        float _HackerMinDrawDistance;
        float _HackerMaxDrawDistance;
        float _HackerFadeRange;
        float _HackerOverallAlpha;

        // Camera depth texture
        TEXTURE2D_X(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        // Helpers
        float textAtUV(float2 uv)
        {
            float2 cell  = frac(uv * 16.0);
            float2 block = uv * 16.0 - cell;
            float2 rndUV = block / _ScreenParams.xy + _Time * _TimeSpeedFont;
            float2 rnd   = SAMPLE_TEXTURE2D_X(_NoiseTex, sampler_NoiseTex, rndUV).xy;
            float2 letter= (floor(rnd * 16.0) + cell) * (1.0/16.0);
            letter.x     = 1.0 - letter.x;
            return SAMPLE_TEXTURE2D_X(_FontTex, sampler_FontTex, letter).r;
        }
        float3 rainAtUV(float2 uv)
        {
            uv.x   = floor(uv.x * 16.0) / 16.0;
            float offset = sin(uv.x * 15.0);
            float speed  = cos(uv.x * 3.0) * 0.3 + 0.7;
            float y      = frac(uv.y + _Time * speed * _TimeSpeedRain + offset);
            y            = max(y, 0.001); // Clamp y to prevent division by zero or very small numbers
            return float3(0.1, 1.0, 0.35) / (y * 20.0);
        }

        float3 ReconstructPosition(in float2 uv, in float z)
        {
            float x = uv.x * 2 - 1;
            float y = (1 - uv.y) * 2 - 1;
            float4 position_s = float4(x, y, z, 1);
            float4 position_v = mul(UNITY_MATRIX_I_VP, position_s);
            return position_v.xyz / position_v.w;
        }

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            // Don't render if we can't see anything anyway
            if (_HackerOverallAlpha <= 0.0005)
                discard;

            SurfaceDescription s = (SurfaceDescription)0;
            float2 uv = IN.NDCPosition;

            // sample depth
            float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
            if (rawDepth >= 0.9999)    // discard sky/empty pixels
                discard;
            
            // Reconstruct world position
            float3 worldPosition = ReconstructPosition(uv, rawDepth);

            // Calculate distance to camera
            float distanceToCamera = length(worldPosition - _WorldSpaceCameraPos.xyz);

            // Calculate distance fade
            float fadeMin = smoothstep(_HackerMinDrawDistance - _HackerFadeRange, _HackerMinDrawDistance + _HackerFadeRange, distanceToCamera);
            float fadeMax = 1.0 - smoothstep(_HackerMaxDrawDistance - _HackerFadeRange, _HackerMaxDrawDistance + _HackerFadeRange, distanceToCamera);
            float distanceAlpha = saturate(fadeMin * fadeMax);

            // If completely faded out by distance, discard early
            if (distanceAlpha <= 0.001)
                discard;
            
            // normal from derivatives
            float3 dPdx = ddx(worldPosition);
            float3 dPdy = ddy(worldPosition);
            float3 n    = normalize(cross(dPdy, dPdx));
            
            // triplanar blend
            float3 blend = abs(n); 
            blend /= (blend.x + blend.y + blend.z);
            
            // planar UVs
            float scale = 1.0;
            float2 uvXY = worldPosition.xy * scale;
            float2 uvYZ = worldPosition.yz * scale;
            float2 uvZX = worldPosition.zx * scale;
            
            // sample & combine
            float3 cXY = textAtUV(uvXY) * rainAtUV(uvXY);
            float3 cYZ = textAtUV(uvYZ) * rainAtUV(uvYZ);
            float3 cZX = textAtUV(uvZX) * rainAtUV(uvZX);
            
            s.BaseColor = cXY * blend.z + cYZ * blend.x + cZX * blend.y;
            s.Alpha     = distanceAlpha * _HackerOverallAlpha; // MODIFIED: Apply distance fade and overall alpha
            return s;
        }

        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            output.NDCPosition = input.texCoord0.xy;
        
            return output;
        }

        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenCommon.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenDrawProcedural.hlsl"
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenShaderGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}