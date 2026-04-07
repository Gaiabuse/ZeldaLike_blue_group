Shader "Custom/BlobTransparentGlow"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,0.8,0.5,0.4)
        _ColorIntensity ("Color Intensity", Range(0,5)) = 1
        _Transparency ("Transparency", Range(0,1)) = 0.5

        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,5)) = 1
        _RimPower ("Rim Width", Range(0.5,8)) = 3

        _DashCount ("Dash Count", Float) = 20
        _DashSize ("Dash Size", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float4 _Color;
            float _ColorIntensity;
            float _Transparency;

            float4 _GlowColor;
            float _GlowIntensity;
            float _RimPower;

            float _DashCount;
            float _DashSize;

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                o.uv = v.uv;

                return o;
            }

            float DashPattern(float2 uv)
            {
                float pattern = frac(uv.x * _DashCount);
                return step(pattern, _DashSize);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                float3 V = normalize(i.viewDir);

                float rim = 1 - saturate(dot(N, V));
                rim = pow(rim, _RimPower);

                float dash = DashPattern(i.uv);
                float dashedRim = rim * dash;

                float3 baseColor = _Color.rgb * _ColorIntensity;

                float3 glow = _GlowColor.rgb * dashedRim * _GlowIntensity;

                float3 finalColor = baseColor + glow;

                float alpha = saturate(_Transparency + dashedRim * 0.5);

                return float4(finalColor, alpha);
            }

            ENDCG
        }
    }
}