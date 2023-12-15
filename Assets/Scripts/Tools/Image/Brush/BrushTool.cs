using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Scripting;
using XDPaint.Core;
using XDPaint.Core.PaintModes;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    [Serializable]
    public sealed class BrushTool : BasePaintTool<BrushToolSettings>
    {
        [Preserve]
        public BrushTool(IPaintData paintData) : base(paintData)
        {
            Settings = new BrushToolSettings(paintData);
        }

        public override PaintTool Type => PaintTool.Brush;
        public override bool RequiredCombinedTempTexture => Settings.UsePattern;

        public override void Enter()
        {
            base.Enter();
            if (Settings.PatternTexture == null)
            {
                Settings.PatternTexture = Tools.Settings.Instance.DefaultPatternTexture;
            }
            ToolSettingsOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Settings.UsePattern)));
            Settings.PropertyChanged += ToolSettingsOnPropertyChanged;
        }
        
        public override void Exit()
        {
            Settings.PropertyChanged -= ToolSettingsOnPropertyChanged;
            base.Exit();
            Data.Material.DisableKeyword(Constants.PaintShader.TileKeyword);
        }

        public override void SetPaintMode(IPaintMode mode)
        {
            base.SetPaintMode(mode);
            ToolSettingsOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Settings.UsePattern)));
        }
        
        private void ToolSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.UsePattern))
            {
                if (!Data.PaintMode.UsePaintInput && Settings.UsePattern)
                {
                    Debug.LogWarning("PaintMode Default does not supports pattern painting, please, switch to PaintMode Additive.");
                    Settings.usePattern = false;
                    Data.Material.DisableKeyword(Constants.PaintShader.TileKeyword);
                }
                else
                {
                    if (Settings.UsePattern)
                    {
                        Data.Material.EnableKeyword(Constants.PaintShader.TileKeyword);
                    }
                    else
                    {
                        Data.Material.DisableKeyword(Constants.PaintShader.TileKeyword);
                    }
                }
                
                UpdateCombinedTempTexture(false);
            }
        }
    }
}