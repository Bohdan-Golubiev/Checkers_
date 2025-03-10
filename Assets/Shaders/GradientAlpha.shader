Shader "UI/GradientAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeRadius("Fade Radius", Range(0,1)) = 0.5
        _FadeSmooth("Fade Smooth", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FadeRadius;
            float _FadeSmooth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
                // Определяем расстояние от центра UV (0.5,0.5)
                float2 uv = IN.texcoord;
                float dist = distance(uv, float2(0.5, 0.5));
                // smoothstep вернёт 0 при dist <= _FadeRadius и 1 при dist >= (_FadeRadius + _FadeSmooth)
                float fade = smoothstep(_FadeRadius, _FadeRadius + _FadeSmooth, dist);
                // Инвертируем, чтобы в центре было 1, а по краям 0
                fade = 1 - fade;
                col.a *= fade;
                return col;
            }
            ENDCG
        }
    }
}
