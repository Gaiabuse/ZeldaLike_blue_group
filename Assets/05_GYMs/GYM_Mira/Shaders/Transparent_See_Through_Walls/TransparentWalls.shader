Shader "Custom/GlowThroughWalls"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1,0,0,1)
        _GlowIntensity ("Glow Intensity", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _GlowColor;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _GlowColor * _GlowIntensity;
            }
            ENDCG
        }
    }
}