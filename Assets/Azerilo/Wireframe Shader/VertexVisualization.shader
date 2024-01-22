Shader "MeshEdit/VertexVisualization"
{
	 Properties
    {
        _PointSize ("Point Size", Range (0.1, 100)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 xboxone ps4 switch
            ENDCG

            SetTexture[_CameraColorTexture]
            {
                combine primary
            }
        }
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 xboxone ps4 switch
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
            };

            uniform float _PointSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate the point size in screen space
                o.pos = ComputeGrabScreenPos(o.pos);
                o.pos.xy /= o.pos.w;
                o.pos.xy *= _PointSize * 0.005 * UNITY_MATRIX_P[0][0];

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG

            SetTexture[_CameraColorTexture]
            {
                combine primary
            }
        }
    }
}