Shader "Custom/GradientCloud"
{
    Properties
    {
        _LightPink ("Light Pink (Highlight)", Color) = (1,0.85,0.9,1)
        _LightOrange ("Light Orange (Mid Highlight)", Color) = (1,0.75,0.55,1)
        _DarkPink ("Dark Pink (Mid Shadow)", Color) = (0.9,0.4,0.6,1)
        _DarkPurple ("Dark Purple (Shadow)", Color) = (0.4,0.2,0.5,1)

        _GradientSmooth ("Gradient Smoothness", Range(0.1,5)) = 1
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

            float4 _LightPink;
            float4 _LightOrange;
            float4 _DarkPink;
            float4 _DarkPurple;

            float _GradientSmooth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = dot(normal, lightDir);

                NdotL = saturate(NdotL);
                NdotL = pow(NdotL, _GradientSmooth);

                float3 color;

                if (NdotL > 0.75)
                {
                    color = lerp(_LightOrange.rgb, _LightPink.rgb, (NdotL - 0.75) * 4);
                }
                else if (NdotL > 0.5)
                {
                    color = lerp(_DarkPink.rgb, _LightOrange.rgb, (NdotL - 0.5) * 4);
                }
                else if (NdotL > 0.25)
                {
                    color = lerp(_DarkPurple.rgb, _DarkPink.rgb, (NdotL - 0.25) * 4);
                }
                else
                {
                    color = _DarkPurple.rgb;
                }

                return float4(color,1);
            }

            ENDCG
        }
    }
}
