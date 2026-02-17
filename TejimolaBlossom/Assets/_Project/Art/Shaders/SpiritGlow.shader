Shader "Tejimola/SpiritGlow"
{
    // Used for spirit/ethereal objects in Act III and Dom's pulse
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.294, 0, 0.510, 1)
        _GlowColor ("Glow Color", Color) = (0.5, 0.3, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
        _GlowSize ("Glow Size", Range(0, 0.1)) = 0.02
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        _Alpha ("Alpha", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+1"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize;
            float _PulseSpeed;
            float _PulseAmount;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Glow: sample neighbors for bloom effect
                float glowAlpha = 0;
                int samples = 12;
                for (int j = 0; j < samples; j++)
                {
                    float angle = (float)j / (float)samples * 6.28318;
                    float2 offset = float2(cos(angle), sin(angle)) * _GlowSize;
                    glowAlpha += tex2D(_MainTex, i.uv + offset).a;
                }
                glowAlpha /= samples;

                // Pulsing
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                // Compose
                if (col.a > 0.1)
                {
                    // Tint the sprite
                    col.rgb = lerp(col.rgb, _Color.rgb, 0.5);
                    col.a *= _Alpha * pulse;
                }
                else if (glowAlpha > 0.05)
                {
                    // Glow around sprite
                    col = _GlowColor;
                    col.a = glowAlpha * _GlowIntensity * pulse * 0.5;
                }

                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
