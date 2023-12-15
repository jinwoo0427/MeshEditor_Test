namespace XDPaint.Editor
{
    public static class BrushDrawerHelper
    {
        public const float MinAngleValue = 0f;
        public const float MaxAngleValue = 360f;
        public const float MinHardnessValue = -1f;
        public const float MaxHardnessValue = 1f;
        public const float MinValue = 0.01f;
        public const float MaxValue = 8f;
        public const float MinBrushTextureSize = 1f;
        public const float MaxBrushTextureSize = 192f;
        public const string TextureTooltip = "Texture";
        public const string FilterTooltip = "Filter Mode";
        public const string ColorTooltip = "Color of brush";
        public const string NameTooltip = "Name of brush";
        public const string SizeTooltip = "Scale of brush";
        public const string RenderAngleTooltip = "Brush angle";
        public const string HardnessTooltip = "Hardness of brush";
        public const string PreviewTooltip = "If enabled, will be active preview of the brush";
        public const int PropertiesCount = 9;
        public const float LineOffset = 5f;
        public const string DefaultPresetName = "Common";
        public const string CustomPresetName = "Custom";
    }
}