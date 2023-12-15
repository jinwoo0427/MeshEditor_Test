namespace XDPaint.Core
{
    public enum ObjectComponentType
    {
        Unknown,
        RawImage,
        MeshFilter,
        SkinnedMeshRenderer,
        SpriteRenderer
    }

    public enum PaintPass
    {
        Paint = 0,
        Blend = 1,
        Erase = 2,
        Preview = 3
    }
    
    public enum PaintTool
    {
        Brush,
        Erase,
        Eyedropper,
        Bucket
    }

    public enum PaintRenderTexture
    {
        PaintTexture,
        CombinedTexture
    }

    public enum PaintMode
    {
        Default = 0x0,
        Additive = 0x100
    }
    
    public enum RenderTarget
    {
        ActiveLayer = 0x100,
        ActiveLayerTemp = 0x150,
        Input = 0x200,
        Combined = 0x300,
        CombinedTemp = 0x350
    }

    //public enum BlendingMode
    //{
    //    Normal,
        
    //    Darken,
    //    Multiply,
    //    ColorBurn,
    //    LinearBurn,
    //    DarkerColor,
        
    //    Lighten,
    //    Screen,
    //    ColorDodge,
    //    LinearDodge,
    //    LighterColor,
        
    //    Overlay,
    //    SoftLight,
    //    HardLight,
    //    VividLight,
    //    LinearLight,
    //    PinLight,
    //    HardMix,
        
    //    Difference,
    //    Exclusion,
    //    Subtract,
    //    Divide,
        
    //    Hue,
    //    Saturation,
    //    Color,
    //    Luminosity
    //}
}