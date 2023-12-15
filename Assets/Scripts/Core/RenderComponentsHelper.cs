using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Tools;

namespace XDPaint.Core
{
    [System.Serializable]
    public class RenderComponentsHelper : IRenderComponentsHelper
    {
        public ObjectComponentType ComponentType { get; private set; }
        public Component PaintComponent { get; private set; }
        public Component RendererComponent { get; private set; }

        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");

        public Material Material
        {
            get
            {
                if (ComponentType == ObjectComponentType.RawImage)
                {
                    return ((RawImage) RendererComponent).material;
                }
                return ((Renderer) RendererComponent).sharedMaterial;
            }
            set
            {
                var rawImage = PaintComponent as RawImage;
                if (rawImage != null)
                {
                    rawImage.material = value;
                    return;
                }

                var rendererComponent = PaintComponent as Renderer;
                if (rendererComponent != null)
                {
                    rendererComponent.sharedMaterial = value;
                }
            }
        }

        public void Init(GameObject gameObject, out ObjectComponentType componentType)
        {
            if (gameObject.TryGetComponent<RawImage>(out var canvasImage))
            {
                PaintComponent = canvasImage;
                RendererComponent = PaintComponent;
                ComponentType = componentType = ObjectComponentType.RawImage;
                return;
            }
            
            if (gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                PaintComponent = meshFilter;
                if (gameObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    RendererComponent = meshRenderer;
                }
                ComponentType = componentType = ObjectComponentType.MeshFilter;
                return;
            }
            
            if (gameObject.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                PaintComponent = skinnedMeshRenderer;
                RendererComponent = PaintComponent;
                ComponentType = componentType = ObjectComponentType.SkinnedMeshRenderer;
                return;
            }

            if (gameObject.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                PaintComponent = spriteRenderer;
                RendererComponent = PaintComponent;
                ComponentType = componentType = ObjectComponentType.SpriteRenderer;
                return;
            }

            Debug.LogError("Can't find render component in ObjectForPainting field!");
            ComponentType = componentType = ObjectComponentType.Unknown;
        }

        public bool IsMesh()
        {
            return RendererComponent is MeshRenderer || RendererComponent is SkinnedMeshRenderer;
        }

        public Texture GetSourceTexture(Material material, string shaderTextureName)
        {
            if (ComponentType == ObjectComponentType.SkinnedMeshRenderer || 
                ComponentType == ObjectComponentType.MeshFilter ||
                ComponentType == ObjectComponentType.RawImage && !material.shader.name.StartsWith("UI/"))
            {
                if (!string.IsNullOrEmpty(shaderTextureName))
                {
                    return material.GetTexture(shaderTextureName);
                }
            }
            else if (ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var spriteRenderer = RendererComponent as SpriteRenderer;
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    return spriteRenderer.sprite.texture;
                }
            }
            else if (ComponentType == ObjectComponentType.RawImage)
            {
                var image = RendererComponent as RawImage;
                if (image != null && image.texture != null)
                {
                    return image.texture;
                }
            }
            return null;
        }

        public Texture CreateSourceTexture(Material material, string shaderTextureName, int width, int height, Color color)
        {
            if (ComponentType == ObjectComponentType.SkinnedMeshRenderer || 
                ComponentType == ObjectComponentType.MeshFilter ||
                ComponentType == ObjectComponentType.RawImage && !material.shader.name.StartsWith("UI/"))
            {
                if (!string.IsNullOrEmpty(shaderTextureName))
                {
                    return CreateTexture(width, height, color);
                }
            }
            else if (ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var spriteRenderer = RendererComponent as SpriteRenderer;
                if (spriteRenderer != null)
                {
                    var texture = CreateTexture(width, height, color);
                    var pixelPerUnit = Settings.Instance.PixelPerUnit;
                    spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one / 2f, pixelPerUnit);
                    return spriteRenderer.sprite.texture;
                }
            }
            else if (ComponentType == ObjectComponentType.RawImage)
            {
                var image = RendererComponent as RawImage;
                if (image != null)
                {
                    image.texture = CreateTexture(width, height, color);
                    return image.texture;
                }
            }
            return null;
        }

        public void DestroySourceTexture(Material material, string shaderTextureName)
        {            
            if (ComponentType == ObjectComponentType.SkinnedMeshRenderer || 
                ComponentType == ObjectComponentType.MeshFilter ||
                ComponentType == ObjectComponentType.RawImage && !material.shader.name.StartsWith("UI/"))
            {
                material.SetTexture(shaderTextureName, null);
            }
            else if (ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var spriteRenderer = RendererComponent as SpriteRenderer;
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = null;
                }
            }
            else if (ComponentType == ObjectComponentType.RawImage)
            {
                var image = RendererComponent as RawImage;
                if (image != null)
                {
                    image.texture = null;
                }
            }
        }

        /// <summary>
        /// Creates clear texture
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private Texture2D CreateTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return texture;
        }
        
        public void SetSourceMaterial(Material material, int index = -1)
        {
            if (ComponentType == ObjectComponentType.SkinnedMeshRenderer || 
                ComponentType == ObjectComponentType.MeshFilter || 
                ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var renderer = RendererComponent as Renderer;
                if (renderer != null)
                {
                    if (index == -1)
                    {
                        renderer.material = material;
                    }
                    else
                    {
                        var sharedMaterials = renderer.sharedMaterials;
                        sharedMaterials[index] = material;
                        renderer.sharedMaterials = sharedMaterials;
                    }
                    
                    var spriteRenderer = renderer as SpriteRenderer;
                    if (spriteRenderer == null) 
                        return;
                    var materialPropertyBlock = new MaterialPropertyBlock();
                    materialPropertyBlock.SetTexture(MainTexture, material.mainTexture);
                    spriteRenderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
            else if (ComponentType == ObjectComponentType.RawImage)
            {
                var image = RendererComponent as RawImage;
                if (image != null)
                {
                    image.material = material;
                    image.texture = material.mainTexture;
                }
            }
        }

        public Mesh GetMesh(IPaintManager paintManager, bool useBakedSkinnedMeshRenderer = false)
        {
            if (IsMesh())
            {
                var meshFilter = PaintComponent as MeshFilter;
                if (meshFilter != null)
                {
                    if (!meshFilter.sharedMesh.isReadable)
                    {
                        Debug.LogWarning("Can't read Mesh, please set 'Read/Write' in Mesh import settings");
                    }
                    return meshFilter.sharedMesh;
                }
                
                var skinnedMeshRenderer = PaintComponent as SkinnedMeshRenderer;
                if (skinnedMeshRenderer != null)
                {
                    if (!useBakedSkinnedMeshRenderer && !skinnedMeshRenderer.sharedMesh.isReadable)
                    {
                        Debug.LogWarning("Can't read SkinnedMesh, please set 'Read/Write Enabled' in SkinnedMesh import settings");
                    }
                    if (useBakedSkinnedMeshRenderer)
                    {
                        return RaycastController.Instance.GetMesh(paintManager);
                    }
                    return skinnedMeshRenderer.sharedMesh;
                }
            }
            Debug.LogError("Can't find MeshFilter or SkinnedMeshRenderer component!");
            return null;
        }

        public int GetMaterialIndex(Material material)
        {
            if (ComponentType == ObjectComponentType.SkinnedMeshRenderer || 
                ComponentType == ObjectComponentType.MeshFilter || 
                ComponentType == ObjectComponentType.SpriteRenderer)
            {
                var renderer = RendererComponent as Renderer;
                if (renderer != null)
                {
                    var index = 0;
                    var sharedMaterials = renderer.sharedMaterials;
                    for (var i = 0; i < sharedMaterials.Length; i++)
                    {
                        if (sharedMaterials[i] == material)
                        {
                            index = i;
                            break;
                        }
                    }
                    return index;
                }
            }
            return -1;
        }
    }
}