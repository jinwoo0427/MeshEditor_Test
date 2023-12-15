using UnityEngine;

namespace XDPaint.Editor
{
    public static class LayerDrawerHelper
    {
        public const string NameLabel = "Name:";
        public const string BlendingModeLabel = "Blending:";
        public const string OpacityLabel = "Opacity:";
        public static readonly Color32 GrayColor = new Color32(190, 190, 190, 255);
        public static readonly Color32 Gray2Color = new Color32(60, 60, 60, 255);
        public static readonly Color SelectRectColor = new Color(30 / 255f, 118 / 255f, 215 / 255f, 1f);
        public static readonly Color MoveRectColor = new Color(0, 1, 1, 0.7f);
    }
}