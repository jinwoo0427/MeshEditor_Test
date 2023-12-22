Shader "Custom/UVVisualization"
{
    Properties
    {
        _Color ("Marker Color", Color) = (1, 1, 1, 1)
        _Size ("Marker Size", Range(0.001, 0.1)) = 0.005
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Overlay"
        }

        Pass
        {
            Name "UV_MARKER"
            Blend SrcAlpha OneMinusSrcAlpha

            Cull Off
            ZWrite On  // Enable writing to the depth buffer
            ZTest Always

            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 ps3
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Size;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                // Visualize UV as a cross marker
                float pointSize = _Size / i.pos.w;
                float2 cross = abs(i.pos.xy / i.pos.w);
                float crossSize = max(cross.x, cross.y);
                float alpha = 1.0 - saturate(crossSize - pointSize);
                fixed4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
