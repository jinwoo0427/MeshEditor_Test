using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XDPaint.Core.Layers;
using XDPaint.Core.Materials;
using XDPaint.Core.PaintModes;
using XDPaint.Tools.Image.Base;
using XDPaint.Tools.Raycast.Data;
using XDPaint.Utils;

namespace XDPaint.Core.PaintObject.Base
{
    public class BasePaintObjectRenderer : IDisposable
    {
        public IBrush Brush { get; set; }
        public IPaintTool Tool { get; set; }
        protected Camera Camera { set => lineDrawer.Camera = value; }
        
        protected Paint PaintMaterial;
        protected IPaintMode PaintMode;
        protected IRenderTextureHelper RenderTextureHelper;
        protected Func<ILayer> ActiveLayer;
        private BaseLineDrawer lineDrawer;
        private Mesh mesh;
        private Mesh quadMesh;
        private CommandBufferBuilder commandBufferBuilder;
        
        public void SetPaintMode(IPaintMode paintMode)
        {
            PaintMode = paintMode;
        }

        public void SetActiveLayer(Func<ILayer> getActiveLayer)
        {
            ActiveLayer = getActiveLayer;
        }

        protected void InitRenderer(IPaintManager paintManager, Camera camera, Paint paint)
        {
            mesh = new Mesh();
            PaintMaterial = paint;
            var sourceTextureSize = new Vector2(paint.SourceTexture.width, paint.SourceTexture.height);
            lineDrawer = new BaseLineDrawer(paintManager);
            lineDrawer.Init(camera, sourceTextureSize, RenderLine);
            commandBufferBuilder = new CommandBufferBuilder("XDPaintObject");
            InitQuadMesh();
        }

        private void InitQuadMesh()
        {
            if (quadMesh == null)
            {
                quadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
            }
        }

        public virtual void DoDispose()
        {
            commandBufferBuilder?.Release();
            if (mesh != null)
            {
                UnityEngine.Object.Destroy(mesh);
            }
            if (quadMesh != null)
            {
                UnityEngine.Object.Destroy(quadMesh);
            }
        }

        protected void ClearTexture(RenderTarget target)
        {
            commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).ClearRenderTarget().Execute();
        }
        
        protected void ClearTexture(RenderTexture renderTexture, Color color)
        {
            commandBufferBuilder.Clear().SetRenderTarget(renderTexture).ClearRenderTarget(color).Execute();
        }

        private RenderTargetIdentifier GetRenderTarget(RenderTarget target)
        {
            return target == RenderTarget.ActiveLayer ? ActiveLayer().RenderTexture : RenderTextureHelper.GetTarget(target);
        }
        
        private void ClearTextureAndRender(RenderTarget target, Mesh drawMesh)
        {
            commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).ClearRenderTarget().DrawMesh(drawMesh, Brush.Material).Execute();
        }
        
        private void ClearTextureAndRenderInstanced(RenderTarget target, Mesh drawMesh, Matrix4x4[] matrix)
        {
            commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(target)).ClearRenderTarget().DrawMeshInstanced(drawMesh, Brush.Material, matrix).Execute();
        }

        private void RenderToTexture(RenderTarget target, Mesh drawMesh)
        {
            if (!Tool.RenderToLayer && target == RenderTarget.ActiveLayer)
                return;
            
            if (!Tool.RenderToInput && target == RenderTarget.Input)
                return;

            commandBufferBuilder.Clear().SetRenderTarget(GetRenderTarget(target)).DrawMesh(drawMesh, Brush.Material).Execute();

            //Colorize PaintInput texture
            if (target == RenderTarget.Input)
            {
                commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(RenderTarget.Input)).DrawMesh(drawMesh, Brush.Material, Brush.Material.passCount - 1).Execute();
            }
        }
        
        private void RenderToTextureInstanced(RenderTarget target, Mesh drawMesh, Matrix4x4[] matrix)
        {
            if (!Tool.RenderToLayer && target == RenderTarget.ActiveLayer)
                return;
            
            if (!Tool.RenderToInput && target == RenderTarget.Input)
                return;

            commandBufferBuilder.Clear().SetRenderTarget(GetRenderTarget(target)).DrawMeshInstanced(drawMesh, Brush.Material, matrix, 0).Execute();

            //Colorize PaintInput texture
            if (target == RenderTarget.Input)
            {
                commandBufferBuilder.Clear().SetRenderTarget(RenderTextureHelper.GetTarget(RenderTarget.Input)).DrawMeshInstanced(drawMesh, Brush.Material, matrix, Brush.Material.passCount - 1).Execute();
            }
        }

        protected void DrawPreProcess()
        {
            if (Tool.DrawPreProcess)
            {
                Tool.OnDrawPreProcess(RenderTextureHelper.GetTarget(RenderTarget.Combined));
            }
        }

        protected void DrawProcess()
        {
            if (Tool.DrawProcess)
            {
                Tool.OnDrawProcess(RenderTextureHelper.GetTarget(RenderTarget.Combined));
            }
        }

        protected void BakeInputToPaint()
        {
            if (Tool.BakeInputToPaint)
            {
                Tool.OnBakeInputToLayer(ActiveLayer().RenderTarget);
            }
        }

        protected void RenderQuad(Vector2 paintPosition, Vector2 renderOffset, float quadScale, bool randomizeAngle = false)
        {
            var center = (Vector3)paintPosition;
            var v1 = center + new Vector3(-Brush.RenderTexture.width, Brush.RenderTexture.height) * quadScale / 2f;
            var v2 = center + new Vector3(Brush.RenderTexture.width, Brush.RenderTexture.height) * quadScale / 2f;
            var v3 = center + new Vector3(Brush.RenderTexture.width, -Brush.RenderTexture.height) * quadScale / 2f;
            var v4 = center + new Vector3(-Brush.RenderTexture.width, -Brush.RenderTexture.height) * quadScale / 2f;
            if (randomizeAngle)
            {
                var quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.forward);
                v1 = quaternion * (v1 - center) + center;
                v2 = quaternion * (v2 - center) + center;
                v3 = quaternion * (v3 - center) + center;
                v4 = quaternion * (v4 - center) + center;
            }

            var scale = new Vector2(PaintMaterial.SourceTexture.width, PaintMaterial.SourceTexture.height);
            v1 = v1 / scale + renderOffset;
            v2 = v2 / scale + renderOffset;
            v3 = v3 / scale + renderOffset;
            v4 = v4 / scale + renderOffset;

            quadMesh.SetVertices(new[] { v1, v2, v3, v4 });
            quadMesh.SetUVs(0, new[] {Vector2.up, Vector2.one, Vector2.right, Vector2.zero});
            GL.LoadOrtho();
            if (Tool.RenderToLayer)
            {
                RenderToTexture(PaintMode.RenderTarget, quadMesh);
            }
            
            if (Tool.RenderToInput)
            {
                RenderToLineTexture(quadMesh);
            }
        }

        protected Vector2[] GetLinePositions(Vector2 fistPaintPos, Vector2 lastPaintPos, RaycastData raycastFirst, RaycastData raycastLast, int fingerId)
        {
            return lineDrawer.GetLinePositions(fistPaintPos, lastPaintPos, raycastFirst, raycastLast, fingerId);
        }

        protected void RenderLine(IList<Vector2> positions, Vector2 renderOffset, Texture brushTexture, float brushSizeActual, IList<float> brushSizes, bool randomizeAngle = false)
        {
            lineDrawer.RenderLine(positions, renderOffset, brushTexture, brushSizeActual, brushSizes, randomizeAngle);
        }

        private void RenderToLineTexture(Mesh renderMesh)
        {
            if (Tool.RenderToInput)
            {
                if (PaintMode.UsePaintInput)
                {
                    RenderToTexture(RenderTarget.Input, renderMesh);
                }
                else
                {
                    ClearTextureAndRender(RenderTarget.Input, renderMesh);
                }
            }
        }
        
        private void RenderToLineTextureInstanced(Mesh renderMesh, Matrix4x4[] matrix)
        {
            if (Tool.RenderToInput)
            {
                if (PaintMode.UsePaintInput)
                {
                    RenderToTextureInstanced(RenderTarget.Input, renderMesh, matrix);
                }
                else
                {
                    ClearTextureAndRenderInstanced(RenderTarget.Input, renderMesh, matrix);
                }
            }
        }

        private void RenderLine(List<Vector3> positions, List<Vector2> uv, List<int> indices, List<Color> colors)
        {
            if (mesh != null)
            {
                mesh.Clear(false);
            }
            mesh.SetVertices(positions);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(indices, 0);
            mesh.SetColors(colors);
            GL.LoadOrtho();
            RenderToTexture(PaintMode.RenderTarget, mesh);
            RenderToLineTexture(mesh);
        }
    }
}