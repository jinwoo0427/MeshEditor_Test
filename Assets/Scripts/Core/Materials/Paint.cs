using System;
using UnityEngine;
using XDPaint.Tools;
using Object = UnityEngine.Object;

namespace XDPaint.Core.Materials
{
    [Serializable]
    public class Paint : IDisposable
    {
        #region Properties and variables
        
        private Material material;
        public Material Material => material;

        [SerializeField] private string shaderTextureName = "_MainTex";
        public string ShaderTextureName
        {
            get => shaderTextureName;
            set => shaderTextureName = value;
        }
        
        [SerializeField] private int defaultTextureWidth = 2048;
        public int DefaultTextureWidth
        {
            get => defaultTextureWidth;
            set => defaultTextureWidth = value;
        }
        
        [SerializeField] private int defaultTextureHeight = 2048;
        public int DefaultTextureHeight
        {
            get => defaultTextureHeight;
            set => defaultTextureHeight = value;
        }
        
        [SerializeField] private Color defaultTextureColor = Color.clear;
        public Color DefaultTextureColor
        {
            get => defaultTextureColor;
            set => defaultTextureColor = value;
        }

        private int materialIndex;
        public int MaterialIndex => materialIndex;

        private Texture sourceTexture;
        public Texture SourceTexture => sourceTexture;

        public Material SourceMaterial;
        private IRenderComponentsHelper renderComponentsHelper;
        private Material objectMaterial;
        private bool isSourceTextureCreated;
        private bool initialized;
        
        #endregion

        public void Init(IRenderComponentsHelper renderComponents, Texture source)
        {
            DoDispose();
            renderComponentsHelper = renderComponents;
            materialIndex = renderComponents.GetMaterialIndex(SourceMaterial);
            if (SourceMaterial != null || SourceMaterial != null && objectMaterial == null)
            {
                objectMaterial = Object.Instantiate(SourceMaterial);
            }
            else if (renderComponentsHelper.Material != null)
            {
                objectMaterial = Object.Instantiate(renderComponentsHelper.Material);
            }
            sourceTexture = renderComponentsHelper.GetSourceTexture(objectMaterial, shaderTextureName);
            if (sourceTexture == null && source == null)
            {
                sourceTexture = renderComponentsHelper.CreateSourceTexture(objectMaterial, shaderTextureName, defaultTextureWidth, defaultTextureHeight, defaultTextureColor);
                isSourceTextureCreated = true;
            }
            else if (source != null)
            {
                sourceTexture = source;
            }
            material = new Material(Settings.Instance.PaintShader)
            {
                mainTexture = sourceTexture
            };
            initialized = true;
        }

        public void DoDispose()
        {
            if (isSourceTextureCreated)
            {
                Object.Destroy(sourceTexture);
                isSourceTextureCreated = false;
                renderComponentsHelper.DestroySourceTexture(objectMaterial, shaderTextureName);
            }
            
            if (objectMaterial != null)
            {
                Object.Destroy(objectMaterial);
                objectMaterial = null;
            }
            
            if (material != null)
            {
                Object.Destroy(material);
                material = null;
            }
            
            initialized = false;
        }

        public void RestoreTexture()
        {
            if (!initialized)
                return;
            if (SourceTexture != null)
            {
                objectMaterial.SetTexture(shaderTextureName, SourceTexture);
            }
            else
            {
                renderComponentsHelper.Material = SourceMaterial;
            }
        }

        public void SetObjectMaterialTexture(Texture texture)
        {
            if (!initialized)
                return;
            objectMaterial.SetTexture(shaderTextureName, texture);
            renderComponentsHelper.SetSourceMaterial(objectMaterial, materialIndex);
        }

        public void SetPreviewTexture(Texture texture)
        {
            if (!initialized)
                return;
            material.SetTexture(Constants.PaintShader.BrushTexture, texture);
        }

        public void SetPaintTexture(Texture texture)
        {
            if (!initialized)
                return;
            material.SetTexture(Constants.PaintShader.PaintTexture, texture);
        }
        
        public void SetInputTexture(Texture texture)
        {
            if (!initialized)
                return;
            material.SetTexture(Constants.PaintShader.InputTexture, texture);
        }

        public void SetPaintPreviewVector(Vector4 brushOffset)
        {
            if (!initialized)
                return;
            material.SetVector(Constants.PaintShader.BrushOffset, brushOffset);
        }
    }
}