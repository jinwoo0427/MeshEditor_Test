using System;
using UnityEngine;
using UnityEngine.Rendering;
using GetampedPaint.Core.PaintModes;
using GetampedPaint.Tools;
using GetampedPaint.Utils;

namespace GetampedPaint.Core.Materials
{
    [Serializable]
    public class Brush : IBrush, IDisposable
    {
        #region Events
        
        public event Action<Color> OnColorChanged;
        public event Action<Texture> OnTextureChanged;
        public event Action<bool> OnPreviewChanged;
        
        #endregion
        
        #region Properties and variables

        [SerializeField] private string name = "Brush 1";
        public string Name
        {
            get => name;
            set => name = value;
        }

        [SerializeField] private Material material;
        public Material Material => material;

        [SerializeField] private FilterMode filterMode = FilterMode.Bilinear;
        public FilterMode FilterMode
        {
            get => filterMode;
            set => filterMode = value;
        }

        [SerializeField] private Color color = Color.white;
        public Color Color => color;

        [SerializeField] private Texture sourceTexture;
        public Texture SourceTexture
        {
            get => sourceTexture;
            set => sourceTexture = value;
        }
                 
        [SerializeField] private RenderTexture renderTexture;
        public RenderTexture RenderTexture => renderTexture;

        private float minSize;
        public float MinSize
        {
            get => minSize;
            private set => minSize = value;
        }

        [SerializeField] private float size = 1f;
        public float Size
        {
            get => size;
            set
            {
                size = value;
                size = Mathf.Clamp(size, minSize, float.MaxValue);
            }
        }

        [SerializeField] private float renderAngle;
        public float RenderAngle
        {
            get => renderAngle;
            set
            {
                if (renderAngle != value)
                {
                    renderAngle = value;
                    renderQuaternion = Quaternion.Euler(0, 0, renderAngle);
                    if (initialized)
                    {
                        Render();
                    }
                }
            }
        }
        
        private Vector2 renderOffset;
        public Vector2 RenderOffset
        {
            get => renderOffset;
            private set => renderOffset = value;
        }

        private Quaternion renderQuaternion; 
        public Quaternion RenderQuaternion => renderQuaternion;

        [SerializeField] private float hardness = 0.9f;
        public float Hardness
        {
            get => hardness;
            set
            {
                hardness = value;
                if (initialized)
                {
                    renderMaterial.SetFloat(Constants.BrushShader.Hardness, hardness);
                    Render();
                }
            }
        }

        [SerializeField] private bool preview;
        public bool Preview
        {
            get => preview;
            set
            {
                if (preview != value)
                {
                    preview = value;
                    OnPreviewChanged?.Invoke(preview);
                }
            }
        }

        private Material renderMaterial;
        private Mesh mesh;
        private CommandBufferBuilder commandBufferBuilder;
        private RenderTargetIdentifier renderTarget;
        private bool initialized;
        
        // 루트 2의 근사값
        private const float SqrtTwo = 1.41421356237309504880168872420969807856967187537694f;
        
        #endregion

        public Brush()
        {
        }

        public Brush(Brush brush)
        {
            material = brush.material;
            color = brush.Color;
            sourceTexture = brush.SourceTexture;
            renderTexture = brush.RenderTexture;
            size = brush.Size;
            renderAngle = brush.renderAngle;
            hardness = brush.hardness;
        }
    
        public void Init(IPaintMode mode)
        {
            if (mode == null)
            {
                Debug.LogError("Mode is null!");
                return;
            }
            OnColorChanged?.Invoke(color);
            OnTextureChanged?.Invoke(renderTexture);
            if (sourceTexture == null)
            {
                sourceTexture = Settings.Instance.DefaultBrush;
            }
            commandBufferBuilder = new CommandBufferBuilder("XDPaintBrush");
            InitRenderTexture();
            InitMaterials();
            Render();
            initialized = true;
        }

        public void DoDispose()
        {
            commandBufferBuilder?.Release();
            if (mesh != null)
            {
                UnityEngine.Object.Destroy(mesh);
                mesh = null;
            }
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
                material = null;
            }
            renderTexture.ReleaseTexture();
            initialized = false;
        }

        public void SetValues(Brush brush)
        {
            name = brush.name;
            if (brush.material != null)
            {
                material = brush.material;
            }
            filterMode = brush.filterMode;
            color = brush.color;
            if (brush.renderTexture != null)
            {
                renderTexture = brush.renderTexture;
            }
            size = brush.size;
            renderAngle = brush.renderAngle;
            hardness = brush.hardness;
            preview = brush.preview;
            if (Application.isPlaying && initialized)
            {
                SetTexture(brush.sourceTexture, true, false);
            }
            else
            {
                sourceTexture = brush.sourceTexture;
            }
        }

        private void InitMesh()
        {
            float x, y;
            if (renderTexture.width > renderTexture.height)
            {
                x = 1f;
                y = renderTexture.height / (float)renderTexture.width;
            }
            else
            {
                x = renderTexture.width / (float)renderTexture.height;
                y = 1f;
            }
            
            var createMesh = false;
            if (mesh == null)
            {
                mesh = new Mesh();
                createMesh = true;
            }
            
            var center = new Vector3(x / 2f, y / 2f, 0);
            mesh.SetVertices(new[]
            {
                renderQuaternion * (new Vector3(0, y, 0) - center) + center,
                renderQuaternion * (new Vector3(x, y, 0) - center) + center,
                renderQuaternion * (new Vector3(x, 0, 0) - center) + center,
                renderQuaternion * (new Vector3(0, 0, 0) - center) + center
            });
            
            if (createMesh)
            {
                mesh.SetUVs(0, new[]
                {
                    Vector2.up, Vector2.one, Vector2.right, Vector2.zero
                });
                mesh.SetTriangles(new[] { 0, 1, 2, 2, 3, 0 }, 0);
                mesh.SetColors(new[] { Color.white, Color.white, Color.white, Color.white });
            }
        }

        private void InitRenderTexture()
        {
            var wideSide = Mathf.Max(sourceTexture.width, sourceTexture.height);
            var shortSide = Mathf.Min(sourceTexture.width, sourceTexture.height);
            var side = Mathf.RoundToInt(wideSide * SqrtTwo);
            minSize = 1f / shortSide;
            var textureSize = side + Constants.BrushShader.RenderTexturePadding;
            if (renderTexture != null && renderTexture.IsCreated())
            {
                renderTexture.Release();
                renderTexture.width = textureSize;
                renderTexture.height = textureSize;
                renderTexture.Create();
            }
            else
            {
                renderTexture = RenderTextureFactory.CreateRenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32, filterMode);
            }
            renderTarget = new RenderTargetIdentifier(renderTexture);
            commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(renderTarget).ClearRenderTarget().Execute();
        }

        private void InitMaterials()
        {
            if (material == null)
            {
                material = new Material(Settings.Instance.BrushShader);
            }
            material.color = color;
            material.mainTexture = renderTexture;
            if (renderMaterial == null)
            {
                renderMaterial = new Material(Settings.Instance.BrushRenderShader);
            }
            renderMaterial.mainTexture = sourceTexture;
            renderMaterial.color = color;
            renderMaterial.SetFloat(Constants.BrushShader.Hardness, hardness);
            SetupMaterial();
        }

        private void SetupMaterial()
        {
            var sourceTexel = sourceTexture.texelSize;
            var renderTexel = renderTexture.texelSize;
            var width = renderTexture.width - sourceTexture.width;
            var height = renderTexture.height - sourceTexture.height;
            var halfSize = new Vector2(width, height) / 2f;
            var scale = new Vector2(sourceTexel.x / renderTexel.x, sourceTexel.y / renderTexel.y);
            
            var offset = new Vector2(sourceTexel.x * halfSize.x, sourceTexel.y * halfSize.y);
            offset.x -= offset.x % sourceTexel.x;
            offset.y -= offset.y % sourceTexel.y;
            
            var texelSize = new Vector4(halfSize.x * renderTexel.x, halfSize.y * renderTexel.y, halfSize.x * renderTexel.x, halfSize.y * renderTexel.y);
            var divX = texelSize.x % renderTexel.x;
            var divY = texelSize.y % renderTexel.y;
            texelSize.x += divX;
            texelSize.y += divY;
            texelSize.z -= divX;
            texelSize.w -= divY;
            
            renderOffset = Vector2.zero;
            
            if (width % 2 == 1)
            {
                renderOffset.x = renderTexel.x / 2f;
            }
            
            if (height % 2 == 1)
            {
                renderOffset.y = renderTexel.y / 2f;
            }

            renderMaterial.SetVector(Constants.BrushShader.TexelSize, texelSize);
            renderMaterial.SetVector(Constants.BrushShader.ScaleUV, scale);
            renderMaterial.SetVector(Constants.BrushShader.Offset, offset);
        }

        public void SetColor(Color colorValue, bool render = true, bool sendToEvent = true)
        {
            color = colorValue;
            if (!initialized) 
                return;
            
            material.color = color;
            renderMaterial.color = color;
            if (render)
            {
                Render();
            }
            if (sendToEvent && OnColorChanged != null)
            {
                OnColorChanged(color);
            }
        }

        public void SetTexture(Texture texture, bool render = true, bool sendToEvent = true, bool canUpdateRenderTexture = true)
        {
            var sourceTextureWidth = 0;
            var sourceTextureHeight = 0;
            if (sourceTexture != null)
            {
                sourceTextureWidth = sourceTexture.width;
                sourceTextureHeight = sourceTexture.height;
            }
            
            sourceTexture = texture;
            if (!initialized)
                return;

            renderMaterial.mainTexture = sourceTexture;
            material.mainTexture = sourceTexture;

            if (canUpdateRenderTexture && (sourceTextureWidth != sourceTexture.width || sourceTextureHeight != sourceTexture.height))
            {
                InitRenderTexture();
                render = true;
            }

            if (render)
            {
                Render();
            }
            
            material.mainTexture = renderTexture;

            if (sendToEvent && OnTextureChanged != null)
            {
                OnTextureChanged(renderTexture);
            }
        }

        public void RenderFromTexture(Texture texture)
        {
            var sourceTextureWidth = 0;
            var sourceTextureHeight = 0;
            if (sourceTexture != null)
            {
                sourceTextureWidth = sourceTexture.width;
                sourceTextureHeight = sourceTexture.height;
            }
            
            if (!initialized)
                return;

            var previousTexture = renderMaterial.mainTexture;
            var previousSource = sourceTexture;
            sourceTexture = texture;
            renderMaterial.mainTexture = texture;
            material.mainTexture = texture;

            if (sourceTextureWidth != sourceTexture.width || sourceTextureHeight != sourceTexture.height)
            {
                InitRenderTexture();
            }

            Render();
            
            renderMaterial.mainTexture = previousTexture;
            sourceTexture = previousSource;
            material.mainTexture = renderTexture;
        }
        
        public void Render()
        {
            InitMesh();
            SetupMaterial();
            commandBufferBuilder.LoadOrtho().Clear().SetRenderTarget(renderTarget).ClearRenderTarget().DrawMesh(mesh, renderMaterial).Execute();
        }

        public Brush Clone()
        {
            var clone = MemberwiseClone() as Brush;
            return clone;
        }
    }
}