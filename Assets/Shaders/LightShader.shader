Shader "UI/LightShader"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (1, 0, 0, 1)
        _Color2 ("Color 2", Color) = (0, 0, 1, 1)
        _OriginX ("Origin X", Range(0, 1)) = 0.5
        _OriginY ("Origin Y", Range(0, 1)) = 0.5
        _Falloff ("Falloff", Range(0, 1.5)) = 0.5
        _Smoothing ("Smoothing", Range(0, 10)) = 0.1
        _AspectRatio ("Aspect Ratio", Float) = 1.0

        // Light mask settings
        // _LightPositions[i].xy = posición UV, .z = radio individual en espacio viewport
        _LightCount ("Light Count", Int) = 0

        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #define MAX_LIGHTS 16

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float4 _Color1;
            float4 _Color2;
            float _OriginX;
            float _OriginY;
            float _Falloff;
            float _Smoothing;
            float _AspectRatio;

            // Light mask uniforms
            // Cada entrada: xy = posición UV de la fuente de luz, z = radio individual
            float4 _LightPositions[MAX_LIGHTS];
            int _LightCount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 origin = float2(_OriginX, _OriginY);
                float2 uv = IN.texcoord;

                // --- Gradiente radial (idéntico a RadialGradient) ---
                float2 aspectCorrectedUV = uv;
                aspectCorrectedUV.x *= _AspectRatio;

                float2 aspectCorrectedOrigin = origin;
                aspectCorrectedOrigin.x *= _AspectRatio;

                float dist = length(aspectCorrectedUV - aspectCorrectedOrigin);

                float smoothWidth = _Smoothing * 0.1;
                float edge = dist / _Falloff;
                float smoothEdge = smoothstep(1.0 - smoothWidth, 1.0 + smoothWidth, edge);
                float t = 1.0 - smoothEdge;

                fixed4 gradient = lerp(_Color2, _Color1, t);
                half4 color = (gradient + _TextureSampleAdd) * IN.color;

                // --- Fuentes de luz secundarias ---
                // En su área empujan el color hacia _Color1 (la zona iluminada),
                // sin borrar el overlay. Se toma la contribución máxima (unión).
                float lightMask = 0.0;

                for (int i = 0; i < _LightCount && i < MAX_LIGHTS; i++)
                {
                    float2 lightUV     = _LightPositions[i].xy;
                    float  lightRadius = _LightPositions[i].z;

                    // Corrección de aspecto igual que el gradiente principal
                    float2 aspectCorrectedLight = lightUV;
                    aspectCorrectedLight.x *= _AspectRatio;

                    float lightDist = length(aspectCorrectedUV - aspectCorrectedLight);

                    // Mismo parámetro de suavizado que el gradiente principal
                    float lightEdge = lightDist / max(lightRadius, 0.0001);
                    float lightContrib = 1.0 - smoothstep(1.0 - smoothWidth, 1.0 + smoothWidth, lightEdge);

                    lightMask = max(lightMask, lightContrib);
                }

                // Donde lightMask = 1 (centro), el color y alpha son los de _Color1
                // (igual que si el gradiente principal estuviera centrado ahí).
                // Donde lightMask = 0 (fuera del radio), nada cambia.
                half4 lightColor = (_Color1 + _TextureSampleAdd) * IN.color;
                color = lerp(color, lightColor, lightMask);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
