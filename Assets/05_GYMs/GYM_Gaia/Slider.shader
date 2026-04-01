Shader "UI/MaskedFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _FillAmount ("Fill", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST; // Nécessaire pour le Tiling/Offset

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            float4 _MaskTex_ST; // Nécessaire pour le Tiling/Offset du masque

            float _FillAmount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                // On applique le Tiling et l'Offset ici
                o.uv = v.uv; 
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Appliquer les coordonnées de texture (Tiling/Offset)
                float2 mainUV = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 maskUV = i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                float maskValue = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV).r;

                // LOGIQUE DE REMPLISSAGE :
                // On affiche le pixel si la valeur du masque est inférieure au FillAmount
                // Si ton masque est un dégradé, 0 (noir) sera le début et 1 (blanc) la fin.
                float fill = step(maskValue, _FillAmount);

                col.a *= fill;

                return col;
            }
            ENDHLSL
        }
    }
}