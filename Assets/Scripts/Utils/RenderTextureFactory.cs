using UnityEngine;

namespace XDPaint.Utils
{
    public static class RenderTextureFactory
    {
        public static RenderTexture CreateRenderTexture(RenderTexture renderTexture, bool create = true)
        {
            var texture = new RenderTexture(renderTexture.width, renderTexture.height, 0, renderTexture.format)
            {
                filterMode = renderTexture.filterMode,
                autoGenerateMips = renderTexture.autoGenerateMips,
                useMipMap = renderTexture.useMipMap,
                anisoLevel = renderTexture.anisoLevel
            };
            if (create && !texture.IsCreated())
            {
                texture.Create();
            }
            return texture;
        }
        
        public static RenderTexture CreateRenderTexture(Texture sourceTexture, bool autoGenerateMips = false, bool useMipMap = false, bool create = true)
        {
            var texture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = sourceTexture.filterMode,
                autoGenerateMips = autoGenerateMips,
                useMipMap = useMipMap,
                anisoLevel = sourceTexture.anisoLevel
            };
            if (create && !texture.IsCreated())
            {
                texture.Create();
            }
            return texture;
        }
        
        public static RenderTexture CreateRenderTexture(int width, int height, int depth = 0, 
            RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, 
            TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool autoGenerateMips = false, bool useMipMap = false, 
            int anisoLevel = 0, bool create = true)
        {
            var texture = new RenderTexture(width, height, depth, format)
            {
                filterMode = filterMode,
                autoGenerateMips = autoGenerateMips,
                useMipMap = useMipMap,
                anisoLevel = anisoLevel,
                wrapMode = wrapMode
            };
            if (create && !texture.IsCreated())
            {
                texture.Create();
            }
            return texture;
        }
        
        public static RenderTexture CreateTemporaryRenderTexture(RenderTexture renderTexture, bool create = true)
        {
            var texture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format);
            if (!texture.IsCreated())
            {
                texture.filterMode = renderTexture.filterMode;
                texture.autoGenerateMips = renderTexture.autoGenerateMips;
                texture.useMipMap = renderTexture.useMipMap;
                texture.anisoLevel = renderTexture.anisoLevel;
            }
            if (create && !texture.IsCreated())
            {
                texture.Create();
            }
            return texture;
        }

        public static RenderTexture CreateTemporaryRenderTexture(int width, int height, int depth = 0, 
            RenderTextureFormat format = RenderTextureFormat.ARGB32, FilterMode filterMode = FilterMode.Point, 
            TextureWrapMode wrapMode = TextureWrapMode.Clamp, bool autoGenerateMips = false, bool useMipMap = false, 
            int anisoLevel = 0, bool create = true)
        {
            var texture = RenderTexture.GetTemporary(width, height, depth, format);
            if (!texture.IsCreated())
            {
                texture.filterMode = filterMode;
                texture.autoGenerateMips = autoGenerateMips;
                texture.useMipMap = useMipMap;
                texture.anisoLevel = anisoLevel;
                texture.wrapMode = wrapMode;
            }
            if (create && !texture.IsCreated())
            {
                texture.Create();
            }
            return texture;
        }
    }
}