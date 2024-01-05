using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace ColorPickerUtil
{
    public class ColorHSV
    {
        public float h, s, v, alpha;

        public ColorHSV()
        {

        }

        public ColorHSV(float h, float s, float v)
        {
            this.h = Mathf.Clamp(h, 0.0f, 360.0f);
            this.s = Mathf.Clamp01(s);
            this.v = Mathf.Clamp01(v);
            this.alpha = 1.0f;
        }

        public ColorHSV(float h, float s, float v, float alpha)
        {
            this.h = Mathf.Clamp(h, 0.0f, 360.0f);
            this.s = Mathf.Clamp01(s);
            this.v = Mathf.Clamp01(v);
            this.alpha = Mathf.Clamp01(alpha);
        }

        public ColorHSV(Color color)
        {
            FromColor(color);
        }

        public void FromColor(Color color)
        {
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float delta = max - min;

            if (delta == 0.0f) h = 0.0f;
            else if (max == color.r) h = ((color.g - color.b) / delta % 6.0f) * 60.0f;
            else if (max == color.g) h = ((color.b - color.r) / delta + 2.0f) * 60.0f;
            else if (max == color.b) h = ((color.r - color.g) / delta + 4.0f) * 60.0f;
            h = Mathf.Repeat(h, 360.0f);

            if (max == 0.0f) s = 0.0f;
            else s = delta / max;
            
            v = max;
            alpha = color.a;
        }

        public Color ToColor()
        {
            Color color = new Color();
            color.a = alpha;

            float c = v * s;
            float x = c * (1.0f - Mathf.Abs((h / 60.0f) % 2.0f - 1.0f));
            float m = v - c;

            if (h == 360.0f) h = 0.0f;
            if (h < 60)          { color.r = c; color.g = x; color.b = 0.0f; }
            else if (h < 120.0f) { color.r = x; color.g = c; color.b = 0.0f; }
            else if (h < 180.0f) { color.r = 0.0f; color.g = c; color.b = x; }
            else if (h < 240.0f) { color.r = 0.0f; color.g = x; color.b = c; }
            else if (h < 300.0f) { color.r = x; color.g = 0.0f; color.b = c; }
            else if (h < 360.0f) { color.r = c; color.g = 0.0f; color.b = x; }

            color.r += m;
            color.g += m;
            color.b += m;

            return color;
        }
    }

    public class ColorLab
    {
        public float L, a, b, alpha;

        public ColorLab()
        {

        }

        public ColorLab(float L, float a, float b)
        {
            this.L = Mathf.Clamp(L, 0.0f, 100.0f);
            this.a = Mathf.Clamp(a, -128.0f, 127.0f);
            this.b = Mathf.Clamp(b, -128.0f, 127.0f);
            this.alpha = 1.0f;
        }

        public ColorLab(float L, float a, float b, float alpha)
        {
            this.L = Mathf.Clamp(L, 0.0f, 100.0f);
            this.a = Mathf.Clamp(a, -128.0f, 127.0f);
            this.b = Mathf.Clamp(b, -128.0f, 127.0f);
            this.alpha = Mathf.Clamp01(alpha);
        }

        public ColorLab(Color color)
        {
            FromColor(color);
        }

        public void FromColor(Color color)
        {
            Matrix4x4 RGB_TO_XYZ = new Matrix4x4();
            RGB_TO_XYZ.SetColumn(0, new Vector4(0.4124564f, 0.2126729f, 0.0193339f, 0.0f));
            RGB_TO_XYZ.SetColumn(1, new Vector4(0.3575761f, 0.7151522f, 0.1191920f, 0.0f));
            RGB_TO_XYZ.SetColumn(2, new Vector4(0.1804375f, 0.0721750f, 0.9503041f, 0.0f));
            RGB_TO_XYZ.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            float E = 0.008856f;
            float K = 903.3f;

            //rgb to xyz
            Vector4 xyz = color;
            if (xyz.x <= 0.04045f) xyz.x = xyz.x / 12.92f;
            else xyz.x = Mathf.Pow((xyz.x + 0.055f) / 1.055f, 2.4f);
            if (xyz.y <= 0.04045f) xyz.y = xyz.y / 12.92f;
            else xyz.y = Mathf.Pow((xyz.y + 0.055f) / 1.055f, 2.4f);
            if (xyz.z <= 0.04045f) xyz.z = xyz.z / 12.92f;
            else xyz.z = Mathf.Pow((xyz.z + 0.055f) / 1.055f, 2.4f);

            xyz = RGB_TO_XYZ * xyz;

            //xyz to lab
            xyz.x = xyz.x / 0.950456f;
            xyz.z = xyz.z / 1.088754f;

            if (xyz.x > E) xyz.x = Mathf.Pow(xyz.x, 0.3333333f);
            else xyz.x = (K * xyz.x + 16.0f) / 116.0f;
            if (xyz.y > E) xyz.y = Mathf.Pow(xyz.y, 0.3333333f);
            else xyz.y = (K * xyz.y + 16.0f) / 116.0f;
            if (xyz.z > E) xyz.z = Mathf.Pow(xyz.z, 0.3333333f);
            else xyz.z = (K * xyz.z + 16.0f) / 116.0f;

            L = 116.0f * xyz.y - 16.0f;
            a = 500.0f * (xyz.x - xyz.y);
            b = 200.0f * (xyz.y - xyz.z);
            alpha = color.a;
        }

        public Color ToColor()
        {
            Matrix4x4 XYZ_TO_RGB = new Matrix4x4();
            XYZ_TO_RGB.SetColumn(0, new Vector4(3.2404542f, -0.9692660f, 0.0556434f, 0.0f));
            XYZ_TO_RGB.SetColumn(1, new Vector4(-1.5371385f, 1.8760108f, -0.2040259f, 0.0f));
            XYZ_TO_RGB.SetColumn(2, new Vector4(-0.4985314f, 0.0415560f, 1.0572252f, 0.0f));
            XYZ_TO_RGB.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            float E = 0.008856f;
            float K = 903.3f;

            //lab to xyz
            Vector4 xyz = new Vector4();
            xyz.w = 1.0f;
            xyz.y = (L + 16.0f) / 116.0f;
            xyz.z = xyz.y - (b / 200.0f);
            xyz.x = a / 500.0f + xyz.y;

            if (Mathf.Pow(xyz.x, 3.0f) > E) xyz.x = Mathf.Pow(xyz.x, 3.0f);
            else xyz.x = (116.0f * xyz.x - 16.0f) / K;
            if (L > K * E) xyz.y = Mathf.Pow((L + 16.0f) / 116.0f, 3.0f);
            else xyz.y = L / K;
            if (Mathf.Pow(xyz.z, 3.0f) > E) xyz.z = Mathf.Pow(xyz.z, 3.0f);
            else xyz.z = (116.0f * xyz.z - 16.0f) / K;

            xyz.x = xyz.x * 0.950456f;
            xyz.z = xyz.z * 1.088754f;

            //xyz to rgb
            xyz = XYZ_TO_RGB * xyz;

            if (xyz.x <= 0.0031308f) xyz.x = 12.92f * xyz.x;
            else xyz.x = 1.055f * Mathf.Pow(xyz.x, 0.4166667f) - 0.055f;
            if (xyz.y <= 0.0031308f) xyz.y = 12.92f * xyz.y;
            else xyz.y = 1.055f * Mathf.Pow(xyz.y, 0.4166667f) - 0.055f;
            if (xyz.z <= 0.0031308f) xyz.z = 12.92f * xyz.z;
            else xyz.z = 1.055f * Mathf.Pow(xyz.z, 0.4166667f) - 0.055f;

            Color color = new Color();
            color.r = Mathf.Clamp01(xyz.x);
            color.g = Mathf.Clamp01(xyz.y);
            color.b = Mathf.Clamp01(xyz.z);
            color.a = alpha;

            return color;
        }
    }

    public class ColorHex
    {
        public string hex;
        public float alpha;

        public static bool IsValid(string hex)
        {
            Regex r = new Regex("^[a-fA-F0-9]*$");
            bool valid = hex.Length == 6 && r.IsMatch(hex);
            return valid;
        }

        public ColorHex()
        {
            this.hex = "000000";
        }

        public ColorHex(string hex)
        {
            string add = "";
            for (int i = hex.Length; i < 6; ++i) add += "0";
            hex = add + hex;
            if (IsValid(hex)) this.hex = hex;
            else this.hex = "000000";
            this.alpha = 1.0f;
        }

        public ColorHex(string hex, float alpha)
        {
            string add = "";
            for (int i = hex.Length; i < 6; ++i) add += "0";
            hex = add + hex;
            if (IsValid(hex)) this.hex = hex;
            else this.hex = "000000";
            this.alpha = Mathf.Clamp01(alpha);
        }

        public ColorHex(Color color)
        {
            FromColor(color);
        }

        public void FromColor(Color color)
        {
            Color32 color32 = color;
            string r, g, b;
            r = color32.r.ToString("x2");
            g = color32.g.ToString("x2");
            b = color32.b.ToString("x2");
            hex = r + g + b;
            alpha = color.a;
        }

        public Color ToColor()
        {
            Color32 color32 = new Color();
            color32.r = (byte)int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            color32.g = (byte)int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            color32.b = (byte)int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            Color color = color32;
            color.a = alpha;

            return color;
        }
    }

    public class ColorCMYK
    {
        public float c, m, y, k, alpha;

        public ColorCMYK()
        {

        }

        public ColorCMYK(float c, float m, float y, float k)
        {
            this.c = Mathf.Clamp01(c);
            this.m = Mathf.Clamp01(m);
            this.y = Mathf.Clamp01(y);
            this.k = Mathf.Clamp01(k);
            this.alpha = 1.0f;
        }

        public ColorCMYK(float c, float m, float y, float k, float alpha)
        {
            this.c = Mathf.Clamp01(c);
            this.m = Mathf.Clamp01(m);
            this.y = Mathf.Clamp01(y);
            this.k = Mathf.Clamp01(k);
            this.alpha = Mathf.Clamp01(alpha);
        }

        public ColorCMYK(Color color)
        {
            FromColor(color);
        }

        public void FromColor(Color color)
        {
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            k = 1.0f - max;
            if (max == 0.0f)
            {
                c = m = y = 0.0f;
            }
            else
            {
                c = (max - color.r) / max;
                m = (max - color.g) / max;
                y = (max - color.b) / max;
            }
            alpha = color.a;
        }

        public Color ToColor()
        {
            Color color = new Color();

            color.r = (1.0f - c) * (1.0f - k);
            color.g = (1.0f - m) * (1.0f - k);
            color.b = (1.0f - y) * (1.0f - k);
            color.a = alpha;

            return color;
        }
    }
}