Shader "Custom/PainterlyCloudBlend"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _EffectStrength ("Effect Strength", Range(0,1)) = 1

        _LightPink ("Light Pink (Highlight)", Color) = (1,0.85,0.9,1)
        _LightOrange ("Light Orange", Color) = (1,0.75,0.55,1)
        _DarkPink ("Dark Pink", Color) = (0.9,0.4,0.6,1)
        _DarkPurple ("Dark Purple", Color) = (0.4,0.2,0.5,1)

        _NoiseTex ("Brush Texture", 2D) = "white" {}

        _GradientSmooth ("Gradient Smoothness", Range(0.1,5)) = 1

        _WarpStrength ("Warp Strength", Range(0,0.5)) = 0.15
        _BrushStrength ("Brush Influence", Range(0,1)) = 0.7
        _StepCount ("Color Steps", Range(2,8)) = 4
        _EdgeNoise ("Edge Noise", Range(0,0.2)) = 0.08
        _ColorJitter ("Color Jitter", Range(0,0.2)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex; 
            float _EffectStrength; 

            float4 _LightPink;
            float4 _LightOrange;
            float4 _DarkPink;
            float4 _DarkPurple;

            sampler2D _NoiseTex;

            float _GradientSmooth;
            float _WarpStrength;
            float _BrushStrength;
            float _StepCount;
            float _EdgeNoise;
            float _ColorJitter;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 baseColor = tex2D(_MainTex, i.uv);

                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = saturate(dot(normal, lightDir));
                NdotL = pow(NdotL, _GradientSmooth);

                float2 uv = i.uv;

                float2 warpSample = tex2D(_NoiseTex, uv * 0.5).rg;
                float2 warp = (warpSample - 0.5) * _WarpStrength;

                uv += warp;

                float brush1 = tex2D(_NoiseTex, uv * 2).r;
                float brush2 = tex2D(_NoiseTex, uv * 5).r;

                float brush = lerp(brush1, brush2, 0.4);

                float gradient = NdotL * (1 - _BrushStrength) + brush * _BrushStrength;
                gradient = saturate(gradient);

                gradient = floor(gradient * _StepCount) / _StepCount;

                float edge = tex2D(_NoiseTex, uv * 3).r * _EdgeNoise;

                float3 painterColor;

                if (gradient > 0.75 + edge)
                    painterColor = _LightPink.rgb;
                else if (gradient > 0.5 + edge)
                    painterColor = _LightOrange.rgb;
                else if (gradient > 0.25 + edge)
                    painterColor = _DarkPink.rgb;
                else
                    painterColor = _DarkPurple.rgb;

                float jitter = (brush - 0.5) * _ColorJitter;
                painterColor += jitter;

                float3 finalColor = lerp(baseColor.rgb, painterColor, _EffectStrength);

                return float4(finalColor, 1);
            }

            ENDCG
        }
    }
}