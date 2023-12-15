Shader "XD Paint/Brush Render"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Hardness ("Hardness", Range(-20, 1)) = 0.9
        _TexelSize ("Texel Size", Vector) = (0, 0, 0, 0)
        _ScaleUV ("Scale UV", Vector) = (0, 0, 0, 0)
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0, 0, 0, 0) }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform float4 _MainTex_TexelSize;
            float4 _Color;
            float _Hardness;
            float4 _TexelSize;
            float2 _ScaleUV;
            float2 _Offset;

            float4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv.xy * _ScaleUV - _Offset;
                float4 color = tex2D(_MainTex, uv) * _Color;
                if (i.uv.x <= _TexelSize.z || //left
                    i.uv.x >= 1.0f - _TexelSize.x || //right 
                    i.uv.y <= _TexelSize.w || //bottom
                    i.uv.y >= 1.0f - _TexelSize.y) //top
                {
                    color.a = 0;
                }
                else if (_Hardness < 1.0)
                {
                    float x = 2 * (uv.x - 0.5);
                    float y = 2 * (uv.y - 0.5);
                    float value = x * x + y * y;
                    color.a *= smoothstep(1.0, _Hardness, value);
                }
                return color;
            }
            ENDCG
        }
    }
}