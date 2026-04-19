Shader "Custom/ObjectOutlineDashed"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001,0.05)) = 0.01

        _DashSize ("Dash Size", Float) = 0.5
        _GapSize ("Gap Size", Float) = 0.5
        _DashScale ("Dash Scale", Float) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            float4 _OutlineColor;

            float _DashSize;
            float _GapSize;
            float _DashScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 norm = normalize(v.normal);
                float3 pos = v.vertex.xyz + norm * _OutlineWidth;

                float4 world = mul(unity_ObjectToWorld, float4(pos,1));

                o.worldPos = world.xyz;
                o.pos = UnityObjectToClipPos(float4(pos,1));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pattern = frac((i.worldPos.x + i.worldPos.y + i.worldPos.z) * _DashScale);

                float threshold = _DashSize / (_DashSize + _GapSize);

                if(pattern > threshold)
                    discard;

                return _OutlineColor;
            }

            ENDCG
        }

        Pass
        {
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }

            ENDCG
        }
    }
}