#include "UnityCG.cginc"

float3 RGBToHSL(float3 color)
{
    float3 hsl;
    float fMin = min(min(color.r, color.g), color.b);
    float fMax = max(max(color.r, color.g), color.b);
    float delta = fMax - fMin;
    hsl.z = (fMax + fMin) / 2.0;

    if (delta == 0.0)
    {
        hsl.x = 0.0;
        hsl.y = 0.0;
    }
    else
    {
        if (hsl.z < 0.5)
            hsl.y = delta / (fMax + fMin);
        else
            hsl.y = delta / (2.0 - fMax - fMin);
        
        float deltaR = (((fMax - color.r) / 6.0) + (delta / 2.0)) / delta;
        float deltaG = (((fMax - color.g) / 6.0) + (delta / 2.0)) / delta;
        float deltaB = (((fMax - color.b) / 6.0) + (delta / 2.0)) / delta;

        if (color.r == fMax )
            hsl.x = deltaB - deltaG;
        else if (color.g == fMax)
            hsl.x = (1.0 / 3.0) + deltaR - deltaB;
        else if (color.b == fMax)
            hsl.x = (2.0 / 3.0) + deltaG - deltaR;

        if (hsl.x < 0.0)
            hsl.x += 1.0;
        else if (hsl.x > 1.0)
            hsl.x -= 1.0;
    }
    return hsl;
}

float HueToRGB(float f1, float f2, float hue)
{
    if (hue < 0.0)
        hue += 1.0;
    else if (hue > 1.0)
        hue -= 1.0;
    float res;
    if ((6.0 * hue) < 1.0)
        res = f1 + (f2 - f1) * 6.0 * hue;
    else if ((2.0 * hue) < 1.0)
        res = f2;
    else if ((3.0 * hue) < 2.0)
        res = f1 + (f2 - f1) * ((2.0 / 3.0) - hue) * 6.0;
    else
        res = f1;
    return res;
}

float3 HSLToRGB(float3 hsl)
{
    float3 rgb;
    if (hsl.y == 0.0)
        rgb = float3(hsl.z, hsl.z, hsl.z);
    else
    {
        float f2;
        if (hsl.z < 0.5)
            f2 = hsl.z * (1.0 + hsl.y);
        else
            f2 = (hsl.z + hsl.y) - (hsl.y * hsl.z);
            
        float f1 = 2.0 * hsl.z - f2;
        rgb.r = HueToRGB(f1, f2, hsl.x + (1.0/3.0));
        rgb.g = HueToRGB(f1, f2, hsl.x);
        rgb.b = HueToRGB(f1, f2, hsl.x - (1.0/3.0));
    }
    return rgb;
}

float pinLight(float s, float d)
{
    return 2.0 * s - 1.0 > d ? 2.0 * s - 1.0 : s < 0.5 * d ? 2.0 * s : d;
}
 
float vividLight(float s, float d)
{
    if (s < 0.5)
    {
        s *= 2.0f;
        return s == 0.0 ? s : max(1.0 - ((1.0 - d) / s), 0.0);
    }
    s = 2.0 * (s - 0.5);
    return s == 1.0 ? s : min(d / (1.0 - s), 1.0);
}
 
float hardLight(float s, float d)
{
    return s < 0.5 ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}

float overlay(float s, float d)
{
    return d < 0.5 ? 2.0 * d * s : 1.0 - 2.0 * (1.0 - d) * (1.0 - s);
}

float3 BlendNormal(float3 s, float3 d)
{
    return s;
}

float3 BlendMultiply(float3 s, float3 d)
{
    return s * d;
}
 
float3 ColorBurn(float3 s, float3 d)
{
    return s == 0.0 ? s : max(1.0 - (1.0 - d) / s, 0.0);
}
 
float3 LinearBurn(float3 s, float3 d)
{
    return max(d + s - 1.0, 0.0);
}

float3 Darken(float3 s, float3 d)
{
    return min(s, d);
}

float3 DarkerColor(float3 s, float3 d)
{
    return s.x + s.y + s.z < d.x + d.y + d.z ? s : d;
}
 
float3 Lighten(float3 s, float3 d)
{
    return max(s, d);
}

float3 Screen(float3 s, float3 d)
{
    return 1.0 - (1.0 - d) * (1.0 - s);
}
 
float3 ColorDodge(float3 s, float3 d)
{
    return s == 1.0 ? s : min(d / (1.0 - s), 1.0);
}
 
float3 LinearDodge(float3 s, float3 d)
{
    return min(d + s, 1.0f);
}
 
float3 LighterColor(float3 s, float3 d)
{
    return s.x + s.y + s.z > d.x + d.y + d.z ? s : d;
}
 
float3 Overlay(float3 s, float3 d)
{
    return d < 0.5 ? 2.0 * d * s : 1.0 - 2.0 * (1.0 - d) * (1.0 - s);
}
 
float3 SoftLight(float3 s, float3 d)
{
    return s < 0.5 ? 2.0 * d * s + d * d * (1.0 - 2.0 * s) : sqrt(d) * (2.0 * s - 1.0) + 2.0 * d * (1.0 - s);
}
 
float3 HardLight(float3 s, float3 d)
{
    return s < 0.5 ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
}
 
float3 VividLight(float3 s, float3 d)
{
    float3 color = float3(vividLight(s.x, d.x), vividLight(s.y, d.y), vividLight(s.z, d.z));
    return color;
}
 
float3 LinearLight(float3 s, float3 d)
{
    return s < 0.5 ? LinearBurn(d, s * 2.0f) : LinearDodge(d, (2.0f * (s - 0.5f)));
}
 
float3 PinLight(float3 s, float3 d)
{
    float3 color = float3(pinLight(s.x, d.x), pinLight(s.y, d.y), pinLight(s.z, d.z));
    return color;
}
 
float3 HardMix(float3 s, float3 d)
{
    return VividLight(d, s) < 0.5 ? 0.0 : 1.0;
}

float3 Difference(float3 s, float3 d)
{
    return abs(d - s);
}

float3 Exclusion(float3 s, float3 d)
{
    return s + d - 2.0 * s * d;
}
 
float3 Subtract(float3 s, float3 d)
{
    return d - s;
}
 
float3 Divide(float3 s, float3 d)
{
    return s / d;
}
 
float3 Add(float3 s, float3 d)
{
    return s + d;
}
 
float3 Hue(float3 s, float3 d)
{
    float3 baseHSL = RGBToHSL(d);
    return HSLToRGB(float3(RGBToHSL(s).r, baseHSL.g, baseHSL.b));
}

float3 Saturation(float3 s, float3 d)
{
    float3 baseHSL = RGBToHSL(d);
    return HSLToRGB(float3(baseHSL.r, RGBToHSL(s).g, baseHSL.b));
}
 
float3 Color(float3 s, float3 d)
{
    float3 blendHSL = RGBToHSL(s);
    return HSLToRGB(float3(blendHSL.r, blendHSL.g, RGBToHSL(d).b));
}

float3 Luminosity(float3 s, float3 d)
{
    float3 baseHSL = RGBToHSL(d);
    return HSLToRGB(float3(baseHSL.r, baseHSL.g, RGBToHSL(s).b));
}

float4 AlphaComposite(float4 c1, float op1, float4 c2, float op2)
{
    float ar = op1 + op2 - op1 * op2;
    float asr = op2 / ar;
    float a1 = 1 - asr;
    float a2 = asr * (1 - op1);
    float ab = asr * op1;
    float r = c1.r * a1 + c2.r * a2 + c2.r * ab;
    float g = c1.g * a1 + c2.g * a2 + c2.g * ab;
    float b = c1.b * a1 + c2.b * a2 + c2.b * ab;
    return float4(r, g, b, ar);
}

float2 RotateUV(float2 uv, float degrees)
{
    const float Deg2Rad = UNITY_PI * 2.0 / 360.0;
    float radians = degrees * Deg2Rad;
    float s = sin(radians);
    float c = cos(radians);
    float2x2 rotation = float2x2(c, -s, s, c);
    uv -= 0.5;
    uv = mul(rotation, uv);
    uv += 0.5;
    return uv;
}

#ifdef XDPAINT_LAYER_BLEND_NORMAL
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) BlendNormal(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_DARKEN
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Darken(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_MULTIPLY
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) BlendMultiply(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_COLORBURN
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) ColorBurn(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LINEARBURN
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) LinearBurn(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_DARKERCOLOR
#define XDPAINT_LAYER_BLEND(baseColor, overlayColor) DarkerColor(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LIGHTEN
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Lighten(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_SCREEN
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Screen(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_COLORDODGE
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) ColorDodge(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LINEARDODGE
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) LinearDodge(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LIGHTERCOLOR
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) LighterColor(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_OVERLAY
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Overlay(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_SOFTLIGHT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) SoftLight(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_HARDLIGHT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) HardLight(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_VIVIDLIGHT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) VividLight(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LINEARLIGHT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) LinearLight(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_PINLIGHT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) PinLight(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_HARDMIX
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) HardMix(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_DIFFERENCE
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Difference(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_EXCLUSION
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Exclusion(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_SUBTRACT
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Subtract(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_DIVIDE
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Divide(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_ADD
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Add(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_HUE
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Hue(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_SATURATION
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Saturation(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_COLOR
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Color(baseColor, overlayColor)
#elif XDPAINT_LAYER_BLEND_LUMINOSITY
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) Luminosity(baseColor, overlayColor)
#else
    #define XDPAINT_LAYER_BLEND(baseColor, overlayColor) BlendNormal(baseColor, overlayColor)
#endif