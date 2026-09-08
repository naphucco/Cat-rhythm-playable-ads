Shader "Custom/BackgroundDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Strength ("Strength", Range(0, 0.5)) = 0.15
        _RippleTime ("Ripple Time", Range(0, 3)) = 0.0
        _Thickness ("Ring Thickness", Range(0.5, 10)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Strength;
            float _RippleTime;
            float _Thickness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = 0.5;
                float2 delta = i.uv - center;
                float dist = length(delta);

                float radius1 = _RippleTime * 0.4;
                float radius2 = max(0.0, radius1 - 0.25);

                float wave1 = saturate(1.0 - abs(dist - radius1) * _Thickness);
                wave1 = smoothstep(0.0, 1.0, wave1);

                float wave2 = saturate(1.0 - abs(dist - radius2) * _Thickness);
                wave2 = smoothstep(0.0, 1.0, wave2);

                float combinedWave = wave1 + wave2 * 0.7;
                float offset = combinedWave * _Strength;

                float2 distortedUV = i.uv + delta * offset;
                distortedUV = clamp(distortedUV, 0, 1);

                return tex2D(_MainTex, distortedUV) * i.color;
            }
            ENDCG
        }
    }
}