using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Component = UnityEngine.Component;

namespace XDPaint.Editor
{
    public static class PaintManagerHelper
    {
        public const string TrianglesDataWindowTitle = "Triangles Data";
        public static readonly Vector2 TrianglesDataWindowSize = new Vector2(300, 100);
        public const string BrushTooltip = "Texture of brush";
        public const string CameraTooltip = "GameObject with Camera component";
        public const string MaterialTooltip = "Material for painting";
        public const string ObjectForPaintingTooltip = "Drag here GameObject for painting";
        public const string OverrideCameraTooltip = "Override Camera for transforming screen to world coordinates";
        public const string TextureSizeTip = "Texture size";
        public const string TextureColorTip = "Texture color";
        public const string PaintingToolTooltip = "Painting tool";
        public const string AddLayerTooltip = "Create a new layer above active layer";
        public const string RemoveLayer = "Remove active layer";
        public const string RemoveLayerMask = "Remove layer mask";
        public const string MergeLayers = "Merge selected layer with layer below";
        public const string MergeAllLayers = "Merge all enabled layers into active layer";
        public const string SetNextActiveLayer = "Set active layer to next layer";
        public const string PaintingModeTooltip = "Painting mode";
        public const string FilteringModeTooltip = "Filtering mode";
        public const string UseNeighborsVerticesForRaycastTooltip = "Use neighbors vertices data for raycasts";
        public const string UseSourceTextureAsBackgroundTooltip = "Use source texture as background layer";
        public const string CopySourceTextureToPaintTextureTooltip = "Copies source texture to new layre";
        public const string ShaderTextureNameTooltip = "Shader texture name, which texture will be painted";
        public const string InitializeTip = "Initializes PaintManager";
        public const string TrianglesContainerTooltip = "Save Triangles Data into TrianglesContainer asset";
        public const string CloneMaterialTooltip = "Clones Material into new file";
        public const string CloneTextureTooltip = "Clones selected texture (Shader Texture Name) into new file";
        public const string UndoTooltip = "Undo action";
        public const string RedoTooltip = "Redo action";
        public const string BakeTooltip = "Bake resulting texture to source texture. Note that it will not modify source texture file and stores in memory";
        public const string SaveToFileTooltip = "Save modified texture to file";
        public const string AutoFillButtonTooltip = "Try to fill ObjectForPainting and Material fields automatically";
        private const string FilenamePostfix = " copy";
        private const string DefaultTextureFilename = "Texture.png";
        
        private static readonly string[] TextureImportPlatforms =
        {
            "Standalone", "Web", "iPhone", "Android", "WebGL", "Windows Store Apps", "PS4", "XboxOne", "Nintendo 3DS", "tvOS"
        };
        
        private static readonly Type[] SupportedTypes =
        {
            typeof(RawImage),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(SpriteRenderer)
        };

        public static Component GetSupportedComponent(GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            foreach (var supportedType in SupportedTypes)
            {
                if (gameObject.TryGetComponent(supportedType, out var component))
                {
                    return component;
                }
            }
            return null;
        }
        
        public static bool IsMeshObject(Component component)
        {
            return component is MeshRenderer || component is SkinnedMeshRenderer;
        }

        public static int[] GetSubMeshes(Component component)
        {
            Mesh sharedMesh = null;
            if (component.gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                sharedMesh = meshFilter.sharedMesh;
            }
            if (component.gameObject.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                sharedMesh = skinnedMeshRenderer.sharedMesh;
            }
            if (sharedMesh != null)
            {
                var allSubMeshes = new List<int>();
                for (var i = 0; i < sharedMesh.subMeshCount; i++)
                {
                    allSubMeshes.Add(i);
                }
                return allSubMeshes.ToArray();
            }
            return new []{0};
        }
        
        public static int[] GetUVChannels(Component component)
        {
            Mesh sharedMesh = null;
            if (component.gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                sharedMesh = meshFilter.sharedMesh;
            }
            if (component.gameObject.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                sharedMesh = skinnedMeshRenderer.sharedMesh;
            }

            if (sharedMesh != null)
            {
                var allChannels = new List<int>();
                var uvData = new List<Vector2>();
                for (var i = 0; i < 8; i++)
                {
                    uvData.Clear();
                    sharedMesh.GetUVs(i, uvData);
                    if (uvData.Count > 0)
                    {
                        allChannels.Add(i);
                    }
                }
                return allChannels.ToArray();
            }
            return new[]{0};
        }
        
        public static string[] GetTexturesListFromShader(Material objectMaterial)
        {
            var allTexturesNames = new List<string>();
            var shader = objectMaterial.shader;
            var propertyCount = ShaderUtil.GetPropertyCount(shader);
            for (var i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    allTexturesNames.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return allTexturesNames.ToArray();
        }

        public static Material CloneMaterial(Material material)
        {
            var materialPath = AssetDatabase.GetAssetPath(material);
            var directoryName = Path.GetDirectoryName(materialPath);
            var fileName = Path.GetFileNameWithoutExtension(materialPath);
            var extension = Path.GetExtension(materialPath);
            string materialNewPath;
            do
            {
                fileName += FilenamePostfix;
                materialNewPath = Path.Combine(directoryName, fileName);
            } while (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine(directoryName, fileName + extension)));

            materialNewPath += extension;
            if (AssetDatabase.CopyAsset(materialPath, materialNewPath))
            {
                return AssetDatabase.LoadAssetAtPath<Material>(materialNewPath);
            }
            return null;
        }

        public static void CloneTexture(Material material, string textureName)
        {
            var texture = material.GetTexture(textureName);
            if (texture != null)
            {
                var texturePath = AssetDatabase.GetAssetPath(texture);
                var directoryName = Path.GetDirectoryName(texturePath);
                var fileName = Path.GetFileNameWithoutExtension(texturePath);
                var extension = Path.GetExtension(texturePath);
                string textureNewPath;
                do
                {
                    fileName += FilenamePostfix;
                    textureNewPath = Path.Combine(directoryName, fileName);
                } while (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine(directoryName, fileName + extension)));

                textureNewPath += extension;
                if (AssetDatabase.CopyAsset(texturePath, textureNewPath))
                {
                    var newTexture = AssetDatabase.LoadAssetAtPath<Texture>(textureNewPath);
                    material.SetTexture(textureName, newTexture);
                }
            }
        }

        public static void SaveResultTextureToFile(PaintManager paintManager)
        {
            var sourceTexture = paintManager.Material.Material.mainTexture;
            var texturePath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(texturePath))
            {
                texturePath = Application.dataPath + "/" + DefaultTextureFilename;
            }
            var textureImporterSettings = new TextureImporterSettings();
            var assetImporter = AssetImporter.GetAtPath(texturePath);
            var defaultPlatformSettings = new TextureImporterPlatformSettings();
            var platformsSettings = new Dictionary<string, TextureImporterPlatformSettings>();
            if (assetImporter != null)
            {
                var textureImporter = (TextureImporter)assetImporter;
                textureImporter.ReadTextureSettings(textureImporterSettings);
                defaultPlatformSettings = textureImporter.GetDefaultPlatformTextureSettings();
                foreach (var platform in TextureImportPlatforms)
                {
                    var platformSettings = textureImporter.GetPlatformTextureSettings(platform);
                    if (platformSettings != null)
                    {
                        platformsSettings.Add(platform, platformSettings);
                    }
                }
            }
            
            var directoryInfo = new FileInfo(texturePath).Directory;
            if (directoryInfo != null)
            {
                var directory = directoryInfo.FullName;
                var fileName = Path.GetFileName(texturePath);
                var path = EditorUtility.SaveFilePanel("Save texture as PNG", directory, fileName, "png");
                if (path.Length > 0)
                {
                    var texture2D = paintManager.GetResultTexture();
                    var pngData = texture2D.EncodeToPNG();
                    if (pngData != null)
                    {
                        File.WriteAllBytes(path, pngData);
                    }
                    
                    var importPath = path.Replace(Application.dataPath, "Assets");
                    var importer = AssetImporter.GetAtPath(importPath);
                    if (importer != null)
                    {
                        var texture2DImporter = (TextureImporter)importer;
                        texture2DImporter.SetTextureSettings(textureImporterSettings);
                        texture2DImporter.SetPlatformTextureSettings(defaultPlatformSettings);
                        foreach (var platform in platformsSettings)
                        {
                            texture2DImporter.SetPlatformTextureSettings(platform.Value);
                        }
                        if (!Application.isPlaying)
                        {
                            AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceUpdate);
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }
        }

        public static bool HasTexture(PaintManager paintManager)
        {
            if (paintManager.ObjectForPainting.TryGetComponent<RawImage>(out var rawImage))
            {
                return rawImage.texture != null;
            }
            
            if (paintManager.ObjectForPainting.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                return spriteRenderer.sprite != null;
            }

            if (paintManager.ObjectForPainting.TryGetComponent<Renderer>(out _))
            {
                var shaderTextureName = paintManager.Material.ShaderTextureName;
                return paintManager.Material.SourceMaterial.GetTexture(shaderTextureName) != null;
            }
            return false;
        }

        public static void SetModelReadable(Mesh mesh)
        {
            if (mesh == null)
                return;
            var importPath = AssetDatabase.GetAssetPath(mesh);
            var importer = AssetImporter.GetAtPath(importPath) as ModelImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                Debug.LogWarning($"Enabled Read/Write for model {mesh} at path: {importPath}");
            }
            if (!Application.isPlaying)
            {
                AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }
    }
}