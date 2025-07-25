Shader "Custom/UI/OpenPackBackgroundUIShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // ----- RADIAL GRADIENT -----
        [Header(Radial Gradient)]
        _RadialPivot("Radial Pivot", Vector) = (0, 0.0, 0, 0)
        _RadialGradientBias("Radial Gradient Bias", Range(0, 1.5)) = 1.0
        _InsideColor ("Inside Color", Color) = (0,0,0,0)
        _OutsideColor ("Outside Color", Color) = (1,1,1,1)

        // ----- LINEAR GRADIENT -----
        [Header(Linear Gradient)]
        _LinearGradientRange("Linear Gradient Range", Vector) = (-1.0, 1.0, 0.0, 0.0)
        _LinearGradientBias("Linear Gradient Bias", Range(-0.5, 0.5)) = 1.0
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _MiddleColor("Middle Color", Color) = (0,0,0,1)

        // ----- PATTERN -----
        [Header(Pattern Gradient)]
        _PatternTint ("Pattern Tint", Color) = (1, 1, 1, 1)
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        _ScrollingSpeed ("Scrolling Speed", Vector) = (1.0, 1.0, 0, 0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [Toggle(UNITY_SCREEN_RATIO)] _UseScreenRatio ("Use Screen Ratio", Float) = 1
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
            #pragma multi_compile _ UNITY_SCREEN_RATIO

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
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            // ----- RADIAL GRADIENT -----
            half _RadialGradientBias;
            fixed4 _InsideColor, _OutsideColor;
            float4 _RadialPivot;

            // ----- LINEAR GRADIENT -----
            half _LinearGradientBias;;
            float4 _LinearGradientRange;
            fixed4 _EdgeColor, _MiddleColor;

            // ----- PATTERN -----
            fixed4 _PatternTint;
            sampler2D _PatternTex;
            float4 _PatternTex_ST;
            float4 _ScrollingSpeed;

            fixed4 blendColor(fixed4 top, fixed4 bottom)
            {
                fixed3 rgb = top.rgb * top.a + bottom.rgb * (1.0 - top.a);
                fixed alpha = top.a + bottom.a * (1.0 - top.a);
                return fixed4(rgb, alpha);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 ndcPos = (IN.texcoord.xy - 0.5) * 2.0;

                #ifdef UNITY_SCREEN_RATIO
                ndcPos.x *= _ScreenParams.x / _ScreenParams.y;
                #endif

                float2 uv = (ndcPos.xy + 1.0) / 2.0;
                float dist = distance(ndcPos, _RadialPivot.xy) / _RadialGradientBias;
                fixed4 radialGradientColor = lerp(_InsideColor, _OutsideColor, smoothstep(0, 1, dist));
                fixed4 patternColor = tex2D(_PatternTex, uv * _PatternTex_ST.xy + _PatternTex_ST.zw + _ScrollingSpeed.xy * _Time.x) * _PatternTint;
                float t = smoothstep(_LinearGradientRange.x, _LinearGradientRange.y, ndcPos.y);
                fixed4 topToMiddleColor = lerp(_EdgeColor, _MiddleColor, smoothstep(0, 0.5 - _LinearGradientBias, t));
                fixed4 bottomToMiddleColor = lerp(_MiddleColor, _EdgeColor, smoothstep(0.5 + _LinearGradientBias, 1.0, t));
                fixed4 linearGradientColor = blendColor(bottomToMiddleColor, topToMiddleColor);
                fixed4 color = blendColor(linearGradientColor, radialGradientColor);
                color = fixed4(blendColor(patternColor, color).rgb, 1);

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