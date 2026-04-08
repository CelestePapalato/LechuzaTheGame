Shader "UI/GlowEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowRadius ("Glow Radius", Range(0, 10)) = 2.0
        _GlowStrength ("Glow Strength", Range(0, 5)) = 1.0
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _GlowColor;
                half _GlowRadius;
                half _GlowStrength;
                float4 _ClipRect;
                float4 _MainTex_ST;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = input.color;
                
                // Sample main texture
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Create glow effect using multiple rings to avoid gaps
                half glowAlpha = 0.0;
                half baseAlpha = mainTex.a;
                
                float stepSize = _GlowRadius * 0.01;
                
                // Sample at increasing distances in multiple rings
                // Ring 1: 8 samples
                // Ring 2: 16 samples  
                // Ring 3: 24 samples
                int sampleCount = 0;
                
                for (int ring = 0; ring < 3; ring++)
                {
                    int samplesInRing = 8 + ring * 8;
                    float ringDistance = (ring + 1) * stepSize;
                    
                    for (int i = 0; i < samplesInRing; i++)
                    {
                        float angle = i * PI * 2.0 / samplesInRing;
                        float2 offset = float2(cos(angle), sin(angle)) * ringDistance;
                        half4 sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset);
                        glowAlpha += sample.a;
                        sampleCount++;
                    }
                }
                
                // Average the samples
                glowAlpha /= sampleCount;
                
                // Apply glow strength
                glowAlpha *= _GlowStrength;
                
                // Combine base texture with glow
                half finalAlpha = saturate(baseAlpha + glowAlpha * (1.0 - baseAlpha));
                half3 finalColor = lerp(_GlowColor.rgb * glowAlpha, mainTex.rgb, baseAlpha);
                
                half4 final = half4(finalColor, finalAlpha);
                final *= color;
                
                #ifdef UNITY_UI_CLIP_RECT
                final.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(final.a - 0.001);
                #endif
                
                return final;
            }
            ENDHLSL
        }
    }
}
