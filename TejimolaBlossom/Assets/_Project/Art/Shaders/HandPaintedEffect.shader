Shader "Tejimola/HandPaintedEffect"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.005
        _WobbleAmount ("Wobble Amount", Range(0, 0.02)) = 0.003
        _WobbleSpeed ("Wobble Speed", Range(0, 10)) = 2.0
        _PaperTexture ("Paper Texture", 2D) = "white" {}
        _PaperStrength ("Paper Strength", Range(0, 1)) = 0.15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _WobbleAmount;
            float _WobbleSpeed;
            sampler2D _PaperTexture;
            float _PaperStrength;

            v2f vert (appdata v)
            {
                v2f o;
                // Add subtle wobble for hand-drawn feel
                float wobble = sin(_Time.y * _WobbleSpeed + v.vertex.x * 10.0) * _WobbleAmount;
                v.vertex.xy += wobble;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample main texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Outline detection
                float outlineAlpha = 0;
                float2 offsets[8] = {
                    float2(-1, 0), float2(1, 0), float2(0, -1), float2(0, 1),
                    float2(-1, -1), float2(1, -1), float2(-1, 1), float2(1, 1)
                };

                for (int j = 0; j < 8; j++)
                {
                    float2 offset = offsets[j] * _OutlineWidth;
                    fixed4 neighbor = tex2D(_MainTex, i.uv + offset);
                    outlineAlpha = max(outlineAlpha, neighbor.a);
                }

                // If current pixel is transparent but neighbors aren't, it's an outline pixel
                if (col.a < 0.1 && outlineAlpha > 0.5)
                {
                    col = _OutlineColor;
                    col.a = outlineAlpha;
                }

                // Apply paper texture overlay for hand-painted feel
                fixed4 paper = tex2D(_PaperTexture, i.uv * 4.0);
                col.rgb = lerp(col.rgb, col.rgb * paper.rgb, _PaperStrength);

                // Apply tint
                col *= i.color;
                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
