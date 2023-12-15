Shader "XD Paint/Paint"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _Input ("Input", 2D) = "black" {}
        _Mask ("Mask", 2D) = "white" {}
        _PatternTex ("Pattern", 2D) = "white" {}
        _PatternOffset ("Pattern Offset", Vector) = (0, 0, 0, 0)
        _PatternAngle ("Pattern Angle", float) = 0
        _PatternScale ("Pattern Scale", Vector) = (1, 1, 0, 0)
        _Brush ("Brush", 2D) = "white" {}
        _BrushOffset ("Brush offset", Vector) = (0, 0, 0, 0)
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _Opacity ("Opacity", float) = 1
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Cull Off
        Lighting Off 
        ZWrite Off
        ZTest Always
        Fog { Color (0, 0, 0, 0) }
        Blend Off
        Pass
        {
            //Paint [0]
            CGINCLUDE
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                #ifdef TILE_ON
                float2 uvRotated : TEXCOORD1;
                #endif
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                #ifdef TILE_ON
                o.uvRotated = v.uv;
                #endif
                return o;
            }

            sampler2D _MainTex;
            sampler2D _Input;
            sampler2D _Mask;
            float4 _Color;
            float _Opacity;
            ENDCG

            CGPROGRAM

            #include "BlendingModes.cginc"
            #pragma multi_compile XDPAINT_LAYER_BLEND_NORMAL XDPAINT_LAYER_BLEND_DARKEN XDPAINT_LAYER_BLEND_MULTIPLY XDPAINT_LAYER_BLEND_COLORBURN XDPAINT_LAYER_BLEND_LINEARBURN XDPAINT_LAYER_BLEND_DARKERCOLOR XDPAINT_LAYER_BLEND_LIGHTEN XDPAINT_LAYER_BLEND_SCREEN XDPAINT_LAYER_BLEND_COLORDODGE XDPAINT_LAYER_BLEND_LINEARDODGE XDPAINT_LAYER_BLEND_LIGHTERCOLOR XDPAINT_LAYER_BLEND_OVERLAY XDPAINT_LAYER_BLEND_SOFTLIGHT XDPAINT_LAYER_BLEND_HARDLIGHT XDPAINT_LAYER_BLEND_VIVIDLIGHT XDPAINT_LAYER_BLEND_LINEARLIGHT XDPAINT_LAYER_BLEND_PINLIGHT XDPAINT_LAYER_BLEND_HARDMIX XDPAINT_LAYER_BLEND_DIFFERENCE XDPAINT_LAYER_BLEND_EXCLUSION XDPAINT_LAYER_BLEND_SUBTRACT XDPAINT_LAYER_BLEND_DIVIDE XDPAINT_LAYER_BLEND_HUE XDPAINT_LAYER_BLEND_SATURATION XDPAINT_LAYER_BLEND_COLOR XDPAINT_LAYER_BLEND_LUMINOSITY
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                float4 layer = tex2D(_MainTex, i.uv) * _Color;
                layer.a *= tex2D(_Mask, i.uv).r;
                float4 combined = tex2D(_Input, i.uv);
#ifdef XDPAINT_LAYER_BLEND_NORMAL
                float4 color = AlphaComposite(combined, combined.a, layer, _Opacity * layer.a);
#else
                float4 color = 1;
                color.rgb = XDPAINT_LAYER_BLEND(layer, combined);
                color.a = layer.a;
                color = AlphaComposite(combined, combined.a, color, _Opacity * layer.a);
#endif
                return color;
            }
            ENDCG
        }
        Pass
        {
            //Blend [1]
            CGPROGRAM

            #include "BlendingModes.cginc"
            #pragma multi_compile TILE_OFF TILE_ON XDPAINT_LAYER_BLEND_NORMAL XDPAINT_LAYER_BLEND_DARKEN XDPAINT_LAYER_BLEND_MULTIPLY XDPAINT_LAYER_BLEND_COLORBURN XDPAINT_LAYER_BLEND_LINEARBURN XDPAINT_LAYER_BLEND_DARKERCOLOR XDPAINT_LAYER_BLEND_LIGHTEN XDPAINT_LAYER_BLEND_SCREEN XDPAINT_LAYER_BLEND_COLORDODGE XDPAINT_LAYER_BLEND_LINEARDODGE XDPAINT_LAYER_BLEND_LIGHTERCOLOR XDPAINT_LAYER_BLEND_OVERLAY XDPAINT_LAYER_BLEND_SOFTLIGHT XDPAINT_LAYER_BLEND_HARDLIGHT XDPAINT_LAYER_BLEND_VIVIDLIGHT XDPAINT_LAYER_BLEND_LINEARLIGHT XDPAINT_LAYER_BLEND_PINLIGHT XDPAINT_LAYER_BLEND_HARDMIX XDPAINT_LAYER_BLEND_DIFFERENCE XDPAINT_LAYER_BLEND_EXCLUSION XDPAINT_LAYER_BLEND_SUBTRACT XDPAINT_LAYER_BLEND_DIVIDE XDPAINT_LAYER_BLEND_HUE XDPAINT_LAYER_BLEND_SATURATION XDPAINT_LAYER_BLEND_COLOR XDPAINT_LAYER_BLEND_LUMINOSITY
            #pragma vertex vertex
            #pragma fragment frag

            sampler2D _PatternTex;
            float _PatternAngle;
            float4 _PatternOffset;
            float2 _PatternScale;

            v2f vertex(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                #ifdef TILE_ON
                o.uvRotated.xy = RotateUV(o.uv * _PatternScale, _PatternAngle);
                #endif
                return o;
            }
             
            float4 frag (v2f i) : SV_Target
            {
                float4 paintColor = tex2D(_MainTex, i.uv);
                float4 inputColor = tex2D(_Input, i.uv);
                #ifdef TILE_ON
                inputColor *= tex2D(_PatternTex, i.uvRotated - _PatternOffset);
                #endif
                float4 color = AlphaComposite(paintColor, paintColor.a, inputColor, inputColor.a);
                return color;
            }
            ENDCG
        }
        Pass
        {
            //Erase [2]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                float4 paintColor = tex2D(_MainTex, i.uv);
                float4 inputColor = tex2D(_Input, i.uv);
                paintColor.a -= paintColor.a * inputColor.a;
                return paintColor;
            }
            ENDCG
        }
        Pass
        {
            //Preview [3]
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment frag
            #include "BlendingModes.cginc"
            #pragma multi_compile TILE_OFF TILE_ON
            
            sampler2D _Brush;
            sampler2D _PatternTex;
            float2 _PatternOffset;
            float _PatternAngle;
            float2 _PatternScale;
            float4 _BrushOffset;
                        
            v2f vertex(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                #ifdef TILE_ON
                o.uvRotated.xy = RotateUV(o.uv * _PatternScale, _PatternAngle);
                #endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 paintColor = tex2D(_MainTex, i.uv);
                float4 brushColor = tex2D(_Brush, float2(i.uv.x * _BrushOffset.z - _BrushOffset.x + 0.5f, i.uv.y * _BrushOffset.w - _BrushOffset.y + 0.5f)) * _Color;
                #ifdef TILE_ON
                brushColor *= tex2D(_PatternTex, i.uvRotated - _PatternOffset);
                #endif
                float4 color = AlphaComposite(paintColor, paintColor.a, brushColor, brushColor.a);
                return color;
            }
            ENDCG
        }
    }
}