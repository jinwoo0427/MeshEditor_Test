Shader "XD Paint/Average Color"
{
    Properties {
        _MainTex ("Main", 2D) = "white" {}
        _SourceTex ("SourceTex", 2D) = "white" {}
        _Accuracy ("Accuracy", Int) = 64
    }

    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        ZWrite Off
        ZTest Off
        Lighting Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            uniform float4 _MainTex_ST;
            uniform sampler2D _SourceTex;
            uniform float4 _SourceTex_ST;
            float _Accuracy;

            struct app2vert
            {
                float4 position: POSITION;
                float4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            struct vert2frag
            {
                float4 position: SV_POSITION;
                float4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            vert2frag vert(app2vert input)
            {
                vert2frag output;
                output.position = UnityObjectToClipPos(input.position);
                output.color = input.color;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                return output;
            }

            float4 frag(vert2frag input) : COLOR
            {
                float4 averageColor = float4(0, 0, 0, 0);
                float countSource = 0.0f;

                float stepX = _MainTex_TexelSize.z / _Accuracy;
                if (stepX < 1.0f)
                {
                    stepX = 1.0f;
                }

                float stepY = _MainTex_TexelSize.w / _Accuracy;
                if (stepY < 1.0f)
                {
                    stepY = 1.0f;
                }
                
                int samplesCount = 0;
                //sampling RenderTexture to get average color value
                for (int i = 0; i <= _MainTex_TexelSize.z; i += stepX)
                {
                    for (int j = 0; j <= _MainTex_TexelSize.w; j += stepY)
                    {
                        float2 newCoord = float2(i / _MainTex_TexelSize.z, j / _MainTex_TexelSize.w);
                        float newCountSource = tex2D(_SourceTex, newCoord).a;
                        countSource += newCountSource;
                        averageColor += tex2D(_MainTex, newCoord) * newCountSource;
                        samplesCount++;
                    }
                }
                countSource /= samplesCount;
                averageColor /= samplesCount;
                averageColor /= countSource;
                return averageColor;
            }
            ENDCG
        }
    }
}
