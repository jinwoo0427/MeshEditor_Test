Shader "XD Paint/Brush"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        
        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcColorBlend ("__srcC", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstColorBlend ("__dstC", Int) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcAlphaBlend ("__srcA", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstAlphaBlend ("__dstA", Int) = 1
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOpColor ("__blendC", Int) = 0
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOpAlpha ("__blendA", Int) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Cull Off 
        Lighting Off 
        ZWrite Off
        ZTest Off
        Pass
        {
            BlendOp [_BlendOpColor], [_BlendOpAlpha]
            Blend [_SrcColorBlend] [_DstColorBlend], [_SrcAlphaBlend] [_DstAlphaBlend]
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
                
            sampler2D _MainTex;
            half4 _MainTex_ST;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
                        
            float4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
        Pass
        {
            BlendOp Min
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
                
            sampler2D _MainTex;
            half4 _MainTex_ST;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
                        
            float4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}