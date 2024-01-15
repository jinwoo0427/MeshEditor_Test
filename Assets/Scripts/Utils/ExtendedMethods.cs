using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GetampedPaint.Utils
{
    public static class ExtendedMethods
    {
        public static Vector2 Clamp(this Vector2 value, Vector2 from, Vector2 to)
        {
            if (value.x < from.x)
            {
                value.x = from.x;
            }
            if (value.y < from.y)
            {
                value.y = from.y;
            }
            if (value.x > to.x)
            {
                value.x = to.x;
            }
            if (value.y > to.y)
            {
                value.y = to.y;
            }
            return value;
        }
        
        public static bool IsNaNOrInfinity(this float value)
        {
            return float.IsInfinity(value) || float.IsNaN(value);
        }
        
        public static void ReleaseTexture(this RenderTexture renderTexture)
        {
            if (renderTexture != null && renderTexture.IsCreated())
            {
                if (RenderTexture.active == renderTexture)
                {
                    RenderTexture.active = null;
                }
                renderTexture.Release();
                Object.Destroy(renderTexture);
            }
        }
        
        public static Texture2D GetTexture2D(this RenderTexture renderTexture)
        {
            var format = TextureFormat.ARGB32;
            if (renderTexture.format == RenderTextureFormat.RFloat)
            {
                format = TextureFormat.RFloat;
            }
            var texture2D = new Texture2D(renderTexture.width, renderTexture.height, format, false);
            var previousRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0, false);
            texture2D.Apply();
            RenderTexture.active = previousRenderTexture;
            return texture2D;
        }

        public static string CapitalizeFirstLetter(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (text.Length == 1)
                return text.ToUpper();
            return text.Remove(1).ToUpper() + text.Substring(1);
        }
        
        public static string ToPascalCase(this string text)
        {
            return Regex.Replace(CapitalizeFirstLetter(text), @"\b\p{Ll}", match => match.Value.ToUpper());
        }

        public static string ToCamelCaseWithSpace(this string text)
        {
            return Regex.Replace(CapitalizeFirstLetter(text), @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1").Trim();
        }
        
        public static bool AreColorsSimilar(this Color32 c1, Color32 c2, int tolerance)
        {
            return Mathf.Abs(c1.r - c2.r) <= tolerance && Mathf.Abs(c1.g - c2.g) <= tolerance && 
                   Mathf.Abs(c1.b - c2.b) <= tolerance && Mathf.Abs(c1.a - c2.a) <= tolerance;
        }
        
        public static Bounds TransformBounds(this Bounds localBounds, Transform transform)
        {
            var centerWorld = transform.TransformPoint(localBounds.center);
            var extentsWorld = transform.TransformVector(localBounds.extents);
            extentsWorld.x = Mathf.Abs(extentsWorld.x);
            extentsWorld.y = Mathf.Abs(extentsWorld.y);
            extentsWorld.z = Mathf.Abs(extentsWorld.z);
            return new Bounds(centerWorld, extentsWorld * 2f);
        }

        public static int GetVertexAttributeFormatSize(this VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                    return 4;
                case VertexAttributeFormat.Float16:
                    return 2;
                case VertexAttributeFormat.UNorm8:
                    return 1;
                case VertexAttributeFormat.SNorm8:
                    return 1;
                case VertexAttributeFormat.UNorm16:
                    return 2;
                case VertexAttributeFormat.SNorm16:
                    return 2;
                case VertexAttributeFormat.UInt8:
                    return 1;
                case VertexAttributeFormat.SInt8:
                    return 1;
                case VertexAttributeFormat.UInt16:
                    return 2;
                case VertexAttributeFormat.SInt16:
                    return 2;
                case VertexAttributeFormat.UInt32:
                    return 4;
                case VertexAttributeFormat.SInt32:
                    return 4;
            }
            return 0;
        }

        public static bool IsCorrectFilename(this string filename, bool printIncorrectCharacters)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                if (printIncorrectCharacters)
                {
                    Debug.LogWarning($"Invalid filename! Filename cannot be null or consists only of white-space characters.");
                }
                return false;
            }
            
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                if (printIncorrectCharacters)
                {
                    var invalidChars = new string(filename.Where(c => Path.GetInvalidFileNameChars().Contains(c)).ToArray());
                    Debug.LogWarning($"Invalid filename! Filename contains characters: {invalidChars}");
                }
                return false;
            }
            return true;
        }
    }
}