using System;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using XDPaint.Core;
using XDPaint.Core.PaintModes;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    [Serializable]
    public sealed class EraseTool : BasePaintTool<EraseToolSettings>
    {
        [Preserve]
        public EraseTool(IPaintData paintData) : base(paintData)
        {
            Settings = new EraseToolSettings(paintData);
        }

        public override PaintTool Type => PaintTool.Erase;
        public override bool DrawPreProcess => true;
        public override bool RenderToLayer => !Data.PaintMode.UsePaintInput;
        protected override PaintPass InputToPaintPass => PaintPass.Erase;

        public override void Enter()
        {
            base.Enter();
            SetBrushBlending(Data.PaintMode.UsePaintInput);
        }

        public override void Exit()
        {
            base.Exit();
            base.SetBrushBlending(Data.PaintMode.UsePaintInput);
        }

        public override void SetPaintMode(IPaintMode mode)
        {
            base.SetPaintMode(mode);
            RenderToInput = mode.UsePaintInput;
        }
        
        public override void OnDrawPreProcess(RenderTargetIdentifier combined)
        {
            base.OnDrawPreProcess(combined);
            if (Data.PaintMode.UsePaintInput && Data.IsPainted)
            {
                Data.Material.SetTexture(Constants.PaintShader.PaintTexture, GetTexture(RenderTarget.ActiveLayer));
                Data.CommandBuilder.Clear().SetRenderTarget(GetTarget(RenderTarget.ActiveLayerTemp)).ClearRenderTarget().
                    DrawMesh(Data.QuadMesh, Data.Material, PaintPass.Paint, PaintPass.Erase).Execute();
                Data.Material.SetTexture(Constants.PaintShader.PaintTexture, GetTexture(RenderTarget.ActiveLayerTemp));
            }
        }
        
        protected override void SetBrushBlending(bool usePaintInput)
        {
            if (usePaintInput)
            {
                base.SetBrushBlending(true);
            }
            else
            {
                var material = Data.Brush.Material;
                material.SetInt(Constants.BrushShader.BlendOpColor, (int)BlendOp.Add);
                material.SetInt(Constants.BrushShader.BlendOpAlpha, (int)BlendOp.ReverseSubtract);
                material.SetInt(Constants.BrushShader.SrcColorBlend, (int)BlendMode.Zero);
                material.SetInt(Constants.BrushShader.DstColorBlend, (int)BlendMode.One);
                material.SetInt(Constants.BrushShader.SrcAlphaBlend, (int)BlendMode.SrcAlpha);
                material.SetInt(Constants.BrushShader.DstAlphaBlend, (int)BlendMode.OneMinusSrcAlpha);
            }
        }
    }
}