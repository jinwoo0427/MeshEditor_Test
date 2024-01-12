using System;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.States;
using XDPaint.Utils;

namespace XDPaint.Core.Layers
{
    [Serializable]
    public class Layer : RecordControllerBase, ILayer
    {
        public event Action<ILayer> OnLayerChanged;
        public Action<Layer> OnRenderPropertyChanged;
        
        [SerializeField] private bool enabled = true;
        [UndoRedo] public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value && (value || canLayerBeDisabled()))
                {
                    var previousValue = enabled;
                    enabled = value;
                    OnPropertyChanged(previousValue, value);
                    OnRenderPropertyChanged?.Invoke(this);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }

        public bool CanBeDisabled => (enabled && canLayerBeDisabled()) || !enabled;
        
        [SerializeField] private bool maskEnabled;
        [UndoRedo] public bool MaskEnabled
        {
            get => maskEnabled;
            set
            {
                if (maskEnabled != value && maskRenderTexture != null)
                {
                    var previousValue = maskEnabled;
                    maskEnabled = value;
                    OnPropertyChanged(previousValue, value);
                    OnRenderPropertyChanged?.Invoke(this);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }

        [SerializeField] private string name = "Layer";
        [UndoRedo] public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    var previousValue = name;
                    name = value;
                    OnPropertyChanged(previousValue, value);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }

        [SerializeField] private float opacity = 1f;
        [UndoRedo] public float Opacity
        {
            get => opacity;
            set
            {
                if (opacity != value)
                {
                    var previousValue = opacity;
                    opacity = value;
                    OnPropertyChanged(previousValue, value);
                    OnRenderPropertyChanged?.Invoke(this);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }
        
        [SerializeField] private Texture sourceTexture;
        [UndoRedo] public Texture SourceTexture
        {
            get => sourceTexture;
            set
            {
                if (sourceTexture != value)
                {
                    var previousValue = sourceTexture;
                    sourceTexture = value;
                    OnPropertyChanged(previousValue, value);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }
        
        [SerializeField] private RenderTexture renderTexture;
        [UndoRedo] public RenderTexture RenderTexture
        {
            get => renderTexture;
            private set
            {
                if (renderTexture != value)
                {
                    var oldValue = renderTexture;
                    renderTexture = value;
                    OnPropertyChanged(this, oldValue, renderTexture, sourceTexture);
                    OnRenderPropertyChanged?.Invoke(this);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }

        private RenderTargetIdentifier renderTarget;
        public RenderTargetIdentifier RenderTarget => renderTarget;
        
        [SerializeField] private Texture maskSourceTexture;
        [UndoRedo] public Texture MaskSourceTexture
        {
            get => maskSourceTexture;
            set
            {
                if (maskSourceTexture != value)
                {
                    var previousValue = maskSourceTexture;
                    maskSourceTexture = value;
                    OnPropertyChanged(previousValue, value);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }
        
        [SerializeField] private RenderTexture maskRenderTexture;
        [UndoRedo] public RenderTexture MaskRenderTexture
        {
            get => maskRenderTexture;
            private set
            {
                if (maskRenderTexture != value)
                {
                    var oldValue = maskRenderTexture;
                    maskRenderTexture = value;
                    OnPropertyChanged(this, oldValue, maskRenderTexture, maskSourceTexture);
                    OnRenderPropertyChanged?.Invoke(this);
                    OnLayerChanged?.Invoke(this);
                }
            }
        }
        
        private RenderTargetIdentifier maskRenderTarget;
        public RenderTargetIdentifier MaskRenderTarget => maskRenderTarget;

        //[SerializeField] private BlendingMode blendingMode;
        //[UndoRedo] public BlendingMode BlendingMode
        //{
        //    get => blendingMode;
        //    set
        //    {
        //        if (blendingMode != value)
        //        {
        //            var previousValue = blendingMode;
        //            blendingMode = value;
        //            OnPropertyChanged(previousValue, blendingMode);
        //            OnRenderPropertyChanged?.Invoke(this);
        //            OnLayerChanged?.Invoke(this);
        //        }
        //    }
        //}

        private Func<bool> canLayerBeDisabled;
        private CommandBufferBuilder commandBufferBuilder;

        public Layer(RecordControllerBase recordController) : base(recordController)
        {
        }
        
        public void Create(string layerName, int width, int height, RenderTextureFormat format, FilterMode filterMode)
        {
            Name = layerName;
            renderTexture = RenderTextureFactory.CreateRenderTexture(width, height, 0, format, filterMode);
            renderTexture.name = layerName;
            renderTarget = new RenderTargetIdentifier(renderTexture);
            
            commandBufferBuilder.Clear().SetRenderTarget(renderTarget).ClearRenderTarget(Color.clear).Execute();
            
            OnPropertyChanged(this, null, renderTexture, null, nameof(RenderTexture));
            OnRenderPropertyChanged?.Invoke(this);
        }

        public void Create(string layerName, Texture source, RenderTextureFormat format, FilterMode filterMode)
        {
            Name = layerName;
            sourceTexture = source;
            renderTexture = RenderTextureFactory.CreateRenderTexture(700, 700, 0, format, filterMode);
            renderTexture.name = layerName;
            renderTarget = new RenderTargetIdentifier(renderTexture);
            
            commandBufferBuilder.Clear().SetRenderTarget(renderTarget).ClearRenderTarget(Color.clear).Execute();

            Graphics.Blit(sourceTexture, renderTexture);
            
            OnPropertyChanged(this, null, renderTexture, maskSourceTexture, nameof(RenderTexture));
            OnRenderPropertyChanged?.Invoke(this);
        }
        public void AddImage(Texture source, Rect _dragRect , Vector2 pos)
        {
            //commandBufferBuilder.Clear().SetRenderTarget(renderTarget).ClearRenderTarget(Color.clear).Execute();

            var oldValue = renderTexture;
            Texture2D originalTexture = renderTexture.GetTexture2D();

            Rect adjustedDragRect = AdjustRectWithinTextureBounds(_dragRect, originalTexture);


            Color[] addPixels = ((Texture2D)source).GetPixels(
                0, 
                0, 
                (int)adjustedDragRect.width, 
                (int)adjustedDragRect.height);

            addPixels = ResizeArray(addPixels, Mathf.RoundToInt(adjustedDragRect.width) * Mathf.RoundToInt(adjustedDragRect.height));

            originalTexture.SetPixels(
                (int)pos.x - (int)adjustedDragRect.x, 
                (int)pos.y - (int)adjustedDragRect.y,
                (int)(adjustedDragRect.width),
                (int)(adjustedDragRect.height), addPixels);

            originalTexture.Apply();
            Graphics.Blit(originalTexture, renderTexture);
            OnPropertyChanged(this, oldValue, renderTexture, null, nameof(RenderTexture));
            OnRenderPropertyChanged?.Invoke(this);
        }

        private Rect AdjustRectWithinTextureBounds(Rect originalRect, Texture2D texture)
        {
            // Adjust the rect to fit within the bounds of the target texture
            float maxX = Mathf.Min(originalRect.x + originalRect.width, texture.width);
            float maxY = Mathf.Min(originalRect.y + originalRect.height, texture.height);
            return new Rect(Mathf.Abs( originalRect.x ) , Mathf.Abs( originalRect.y ) , maxX - originalRect.x, maxY - originalRect.y);
        }

        private Color[] ResizeArray(Color[] array, int newSize)
        {
            Color[] newArray = new Color[newSize];
            int length = Mathf.Min(array.Length, newSize);
            System.Array.Copy(array, newArray, length);
            return newArray;
        }
        public Texture2D CombineTexturesFunction(Texture2D baseTex, Texture2D overlayTex)
        {
            // 새로운 텍스처 생성
            Texture2D combinedTexture = new Texture2D(baseTex.width, baseTex.height);

            // 기본 텍스처를 새로운 텍스처에 복사
            combinedTexture.SetPixels(baseTex.GetPixels());

            // 합칠 텍스처를 새로운 텍스처에 덧씌우기
            for (int x = 0; x < combinedTexture.width; x++)
            {
                for (int y = 0; y < combinedTexture.height; y++)
                {
                    Color baseColor = combinedTexture.GetPixel(x, y);
                    Color overlayColor = overlayTex.GetPixel(x, y);

                    // 여기에서 원하는 방식으로 두 색상을 합칩니다.
                    // 예를 들면, 각 색상 채널을 더하거나 곱할 수 있습니다.
                    Color finalColor = baseColor + overlayColor;

                    combinedTexture.SetPixel(x, y, finalColor);
                }
            }

            // 텍스처 적용
            combinedTexture.Apply();

            return combinedTexture;
        }
        public void Init(CommandBufferBuilder bufferBuilder, Func<bool> canDisableLayer)
        {
            commandBufferBuilder = bufferBuilder;
            canLayerBeDisabled = canDisableLayer;
        }
        
        public void AddMask(RenderTextureFormat format)
        {
            maskRenderTexture = RenderTextureFactory.CreateRenderTexture(renderTexture.width, renderTexture.height, 0, format);
            maskRenderTexture.name = $"Mask_{renderTexture.name}";
            maskRenderTarget = new RenderTargetIdentifier(maskRenderTexture);
            
            commandBufferBuilder.Clear().SetRenderTarget(maskRenderTarget).ClearRenderTarget(Color.clear).Execute();

            OnPropertyChanged(this, null, maskRenderTexture, maskSourceTexture, nameof(MaskRenderTexture));
            OnRenderPropertyChanged?.Invoke(this);
        }

        public void AddMask(Texture maskTexture, RenderTextureFormat format)
        {
            maskSourceTexture = maskTexture;
            maskRenderTexture = RenderTextureFactory.CreateRenderTexture(renderTexture.width, renderTexture.height, 0, format);
            maskRenderTexture.name = $"Mask_{renderTexture.name}";
            maskRenderTarget = new RenderTargetIdentifier(maskRenderTexture);    
            
            commandBufferBuilder.Clear().SetRenderTarget(maskRenderTarget).ClearRenderTarget(Color.clear).Execute();
            
            if (maskTexture != null)
            {
                Graphics.Blit(maskSourceTexture, maskRenderTexture);
            }
            OnPropertyChanged(this, null, maskRenderTexture, maskSourceTexture, nameof(MaskRenderTexture));
            OnRenderPropertyChanged?.Invoke(this);
        }

        public void RemoveMask()
        {
            if (maskRenderTexture == null)
                return;
            var oldValue = maskRenderTexture;
            maskRenderTexture = null;
            OnRenderPropertyChanged?.Invoke(this);
            OnPropertyChanged(this, oldValue, maskRenderTexture, maskSourceTexture, nameof(MaskRenderTexture));
        }

        public void SaveState()
        {
            OnPropertyChanged(this, RenderTexture, RenderTexture, sourceTexture, nameof(RenderTexture));
        }
        
        public void DoDispose()
        {
            OnRenderPropertyChanged = null;
            if (renderTexture != null && renderTexture.IsCreated())
            {
                renderTexture.ReleaseTexture();
                renderTexture = null;
            }
            if (maskRenderTexture != null && maskRenderTexture.IsCreated())
            {
                maskRenderTexture.ReleaseTexture();
                maskRenderTexture = null;
            }
            sourceTexture = null;
            RemoveMask();
            commandBufferBuilder = null;
        }
    }
}