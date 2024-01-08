using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;
using XDPaint.Core;
using XDPaint.Core.Layers;
using XDPaint.Core.PaintObject.Data;
using XDPaint.Tools.Images.Base;
using System;

namespace XDPaint.Tools.Images
{
    [Serializable]
    public class SelectionTool : BasePaintTool<SelectionToolSettings>
    {
        [Preserve]
        public SelectionTool(IPaintData paintData) : base(paintData)
        {
            Settings = new SelectionToolSettings(paintData);
        }

        public override PaintTool Type => PaintTool.Selection;
        public override bool RenderToLayer => false;
        public override bool RenderToInput => false;
        public override bool ShowPreview => false;
        private RenderTexture SourceTexture => Settings.UseAllActiveLayers ?
            Data.TexturesHelper.GetTexture(RenderTarget.Combined) :
            Data.LayersController.ActiveLayer.RenderTexture;
        private bool isEnableSelection = false;
        public override void Enter()
        {
            base.Enter();

        }
        public override void Exit()
        {
            base.Exit();
        }

        public override void UpdateHover(PointerData pointerData)
        {
            base.UpdateHover(pointerData);

        }
        public override void UpdatePress(PointerData pointerData)
        {
            base.UpdatePress(pointerData);


        }
        public override void UpdateUp(PointerUpData pointerUpData)
        {
            base.UpdateUp(pointerUpData);
        }

        

        
    }
}