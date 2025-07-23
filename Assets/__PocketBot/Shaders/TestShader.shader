Shader "Unlit/TestShader"
{
    Properties
    {
        [MainTexture] _MainTex("Albedo (RGB)", 2D) = "white" {}
        [MainColor] _Color("HSV Color", Color) = (0.5, 0.25, 0.25, 0.5)
        _Hue("Hue", Range(0.0, 1.0)) = 0.5
        _Saturation("Saturation", Range(0.0, 2.0)) = 1.0
        _Value("Value", Range(0.0, 2.0)) = 1.0
        _Contrast("Contrast", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Hue, _Saturation, _Value, _Contrast;

            float3 applyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(angle);
                //Rodrigues' rotation formula
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }

            float4 LinearRGBToHSV(float4 c)
            {
                float3 sRGBLo = c.rgb * 12.92;
                float3 sRGBHi = (pow(max(abs(c.rgb), 1.192092896e-07), float3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
                float3 Linear = float3(c.rgb <= 0.0031308) ? sRGBLo : sRGBHi;
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 P = lerp(float4(Linear.bg, K.wz), float4(Linear.gb, K.xy), step(Linear.b, Linear.g));
                float4 Q = lerp(float4(P.xyw, Linear.r), float4(Linear.r, P.yzx), step(P.x, Linear.r));
                float D = Q.x - min(Q.w, Q.y);
                float  E = 1e-10;
                return float4(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), Q.x, c.a);
            }

            float4 applyHSBEffect(float4 startColor, float4 hsbc)
            {
                float _Hue = 360 * hsbc.r;
                float _Saturation = hsbc.g * 2;
                float _Brightness = hsbc.b * 2 - 1;
                float _Contrast = hsbc.a * 2;
 
                float4 outputColor = startColor;
                outputColor.rgb = applyHue(outputColor.rgb, _Hue);
                outputColor.rgb = (outputColor.rgb - 0.5) * (_Contrast) + 0.5;
                outputColor.rgb = outputColor.rgb + _Brightness;      
                float3 intensity = dot(outputColor.rgb, float3(0.299,0.587,0.114));
                outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
 
                return outputColor;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float4 hsvc = LinearRGBToHSV(_Color);
                hsvc.x -= 0.5;
                float4 _hsvc = float4(_Hue - 0.5, _Saturation / 2.0, _Value / 2.0, _Contrast);
                col = applyHSBEffect(col, _hsvc);
                return col;
            }
            ENDCG
        }
    }
}
