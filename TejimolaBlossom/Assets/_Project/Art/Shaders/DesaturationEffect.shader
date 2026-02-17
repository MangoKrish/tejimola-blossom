Shader "Tejimola/DesaturationEffect"
{
    // Used for Act II color grading - gradually desaturates the scene
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Desaturation ("Desaturation", Range(0, 1)) = 0.0
        _DarknessTint ("Darkness Tint", Color) = (0.184, 0.310, 0.310, 1)
        _DarknessAmount ("Darkness Amount", Range(0, 1)) = 0.0
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.0
        _VignetteColor ("Vignette Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
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
            fixed4 _Color;
            float _Desaturation;
            fixed4 _DarknessTint;
            float _DarknessAmount;
            float _VignetteStrength;
            fixed4 _VignetteColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // Desaturation
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, float3(gray, gray, gray), _Desaturation);

                // Darkness tint
                col.rgb = lerp(col.rgb, col.rgb * _DarknessTint.rgb, _DarknessAmount);

                // Vignette
                float2 center = i.uv - 0.5;
                float vignette = 1.0 - dot(center, center) * 2.0;
                vignette = saturate(vignette);
                vignette = pow(vignette, 1.0 + _VignetteStrength * 3.0);
                col.rgb = lerp(_VignetteColor.rgb, col.rgb, vignette);

                return col;
            }
            ENDCG
        }
    }
}
