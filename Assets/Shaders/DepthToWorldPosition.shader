Shader "XD Paint/Depth To World Position"
{
    Properties { }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ SOFTPARTICLES_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4x4 _Matrix_IVP;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, o.pos);
                o.screenPos = ComputeScreenPos(o.worldPos);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
#ifdef SOFTPARTICLES_ON
                float2 screenUV = i.screenPos / i.screenPos.w;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

                #if UNITY_REVERSED_Z
                    if (depth < 0.0001)
                        return 0;
                #else
                    if (depth > 0.9999)
                        return 0;
                #endif
                
                float4 positionCS = float4(screenUV * 2.0 - 1.0, depth, 1.0);
                
                #if UNITY_UV_STARTS_AT_TOP
                // positionCS.y = -positionCS.y;
                #endif

                float4 hpositionWS = mul(_Matrix_IVP, positionCS);
                float3 worldPos = hpositionWS.xyz / hpositionWS.w;
                float sceneZ = LinearEyeDepth(depth);
                return float4(worldPos, sceneZ);
#else
                return 0;
#endif
            }

            ENDCG
        }
    }
}