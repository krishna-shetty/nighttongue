Shader "UI/CircleFadeMask"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Center ("Center", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Float) = 0.2
        _Softness ("Softness", Float) = 0.05
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
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = _Center.xy;

                float aspect = _ScreenParams.x / _ScreenParams.y;

                float2 diff = uv - center;
                diff.x *= aspect;

                float dist = length(diff);

                float hole = 1.0 - smoothstep(_Radius - _Softness, _Radius, dist);

                fixed4 col = _Color;
                col.a = _Color.a * (1.0 - hole);

                return col;
            }
            ENDCG
        }
    }
}