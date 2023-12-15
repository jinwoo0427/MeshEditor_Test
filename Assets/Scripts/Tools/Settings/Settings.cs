using UnityEngine;
using XDPaint.Utils;

namespace XDPaint.Tools
{
    [CreateAssetMenu(fileName = "XDPaintSettings", menuName = "XDPaint/Settings", order = 102)]
    public class Settings : SingletonScriptableObject<Settings>
    {
        #region Shaders

        [SerializeField] private Shader brushShader;
        [SerializeField] private Shader brushRenderShader;
        [SerializeField] private Shader eyedropperShader;



        [SerializeField] private Shader paintShader;
        [SerializeField] private Shader averageColorShader;
        [SerializeField] private Shader spriteMaskShader;
        [SerializeField] private Shader depthToWorldPositionShader;
        [SerializeField] private ComputeShader raycastMethod;
        
        public Shader BrushShader => brushShader;
        public Shader BrushRenderShader => brushRenderShader;
        public Shader EyedropperShader => eyedropperShader;

        public Shader PaintShader => paintShader;
        public Shader AverageColorShader => averageColorShader;
        public Shader SpriteMaskShader => spriteMaskShader;
        public Shader DepthToWorldPositionShader => depthToWorldPositionShader;
        public ComputeShader RaycastMethod => raycastMethod;

        #endregion

        public Texture DefaultBrush;
        public Texture DefaultCircleBrush;
        public Texture DefaultPatternTexture;
        public bool PressureEnabled = true;
        public bool CheckCanvasRaycasts = true;
        public RaycastSystemType RaycastsMethod = RaycastSystemType.JobSystem;
        public float BrushDuplicatePartWidth = 16;
        public float PixelPerUnit = 100f;
        public string ContainerGameObjectName = "[XDPaintContainer]";
    }
}