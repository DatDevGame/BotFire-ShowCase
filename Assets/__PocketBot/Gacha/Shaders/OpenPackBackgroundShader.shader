Shader "Custom/Unlit/OpenPackBackground"
{
    Properties
    {
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

        [Toggle(UNITY_SCREEN_RATIO)] _UseScreenRatio ("Use Screen Ratio", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SCREEN_RATIO

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 ndcPos : TEXCOORD1;
            };


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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_ObjectToWorld, v.vertex).xy;
                o.ndcPos = o.vertex.xy / o.vertex.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 ndcPos = i.ndcPos;

                #ifdef UNITY_SCREEN_RATIO
                ndcPos.x *= _ScreenParams.x / _ScreenParams.y;
                #endif

                float dist = distance(ndcPos, _RadialPivot.xy) / _RadialGradientBias;
                fixed4 radialGradientColor = lerp(_InsideColor, _OutsideColor, smoothstep(0, 1, dist));
                fixed4 patternColor = tex2D(_PatternTex, i.uv * _PatternTex_ST.xy + _PatternTex_ST.zw + _ScrollingSpeed.xy * _Time.x) * _PatternTint;
                float t = smoothstep(_LinearGradientRange.x, _LinearGradientRange.y, ndcPos.y);
                fixed4 topToMiddleColor = lerp(_EdgeColor, _MiddleColor, smoothstep(0, 0.5 - _LinearGradientBias, t));
                fixed4 bottomToMiddleColor = lerp(_MiddleColor, _EdgeColor, smoothstep(0.5 + _LinearGradientBias, 1.0, t));
                fixed4 linearGradientColor = blendColor(bottomToMiddleColor, topToMiddleColor);
                fixed4 color = blendColor(linearGradientColor, radialGradientColor);
                return fixed4(blendColor(patternColor, color).rgb, 1);
            }
            ENDCG
        }
    }
}
