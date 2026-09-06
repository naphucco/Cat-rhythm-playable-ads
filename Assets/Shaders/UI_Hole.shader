Shader "Custom/UI/Hole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size", Vector) = (0.4, 0.6, 0, 0)
        _HoleSoftness ("Hole Softness", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
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

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _HoleCenter;
            float4 _HoleSize;
            float _HoleSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                float2 delta = (i.uv - _HoleCenter.xy) / (_HoleSize.xy * 0.5);
                float dist = length(delta);
                float hole = smoothstep(1.0 - _HoleSoftness, 1.0 + _HoleSoftness, dist);

                col.a *= hole;

                return col;
            }
            ENDCG
        }
    }
}