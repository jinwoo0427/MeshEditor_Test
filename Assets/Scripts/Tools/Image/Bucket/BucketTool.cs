using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using GetampedPaint.Core;
using GetampedPaint.Core.PaintObject.Data;
using GetampedPaint.Tools.Images.Base;
using GetampedPaint.Utils;
using Object = UnityEngine.Object;

namespace GetampedPaint.Tools.Images
{
    [Serializable]
    public sealed class BucketTool : BasePaintTool<BucketToolSettings>
    {
        private class BucketData
        {
            public RenderTexture Layer;
            public Vector2Int ClickPosition;
        }

        [Preserve]
        public BucketTool(IPaintData paintData) : base(paintData)
        {
            Settings = new BucketToolSettings(paintData);
        }

        public override PaintTool Type => PaintTool.Bucket;
        public override bool ShowPreview => false;
        public override bool AllowRender => false;
        public override bool ProcessingFinished => false;
        public override bool RequiredCombinedTempTexture => Settings.UsePattern;

        private Texture2D texture;
        private Color32[] pixels;
        private Color32[] filledPixels;
        private bool[] visitedPixels;
        private Queue<BucketData> bucketData;
        private BucketData currentData;
        private Thread thread;
        private bool isRunning;
#if UNITY_WEBGL
        private bool useThreads = false;
#else
        private bool useThreads = true;
#endif

        public override void Enter()
        {
            bucketData = new Queue<BucketData>();
            base.Enter();
            Data.Material.EnableKeyword(Constants.PaintShader.TileKeyword);
            var layerTexture = GetTexture(RenderTarget.ActiveLayer);
            texture = new Texture2D(layerTexture.width, layerTexture.height, TextureFormat.ARGB32, false);
            if (Settings.PatternTexture == null)
            {
                Settings.PatternTexture = Tools.Settings.Instance.DefaultPatternTexture;
            }
            Settings.PropertyChanged += ToolSettingsOnPropertyChanged;
        }

        public override void Exit()
        {
            Settings.PropertyChanged -= ToolSettingsOnPropertyChanged;
            base.Exit();
            Data.Material.DisableKeyword(Constants.PaintShader.TileKeyword);
            Object.Destroy(texture);
            texture = null;
            pixels = null;
            filledPixels = null;
            visitedPixels = null;
            bucketData.Clear();
            currentData = null;
            if (useThreads)
            {
                thread?.Abort();
            }
            isRunning = false;
        }

        public override void UpdateDown(PointerData pointerData)
        {
            base.UpdateDown(pointerData);
            var layerTexture = GetTexture(RenderTarget.ActiveLayer);
            var textureSize = new Vector2Int(layerTexture.width, layerTexture.height);
            var fillPosition = new Vector2Int((int)(pointerData.UV.x * textureSize.x), (int)(pointerData.UV.y * textureSize.y));
            if (fillPosition.x >= 0 && fillPosition.x < texture.width && fillPosition.y >= 0 && fillPosition.y < texture.height)
            {
                var data = new BucketData
                {
                    Layer = layerTexture,
                    ClickPosition = fillPosition
                };
                bucketData.Enqueue(data);
            }
        }

        public override void OnDrawProcess(RenderTargetIdentifier combined)
        {
            var drawProcessed = false;
            if (!isRunning && bucketData.Count > 0)
            {
                base.OnDrawProcess(combined);
                currentData = bucketData.Dequeue();
                var previousRenderTexture = RenderTexture.active;
                RenderTexture.active = currentData.Layer;
                texture.ReadPixels(new Rect(0, 0, currentData.Layer.width, currentData.Layer.height), 0, 0, false);
                texture.Apply();
                RenderTexture.active = previousRenderTexture;
                pixels = texture.GetPixels32();

                if (!Settings.CanFillAlpha && pixels[currentData.ClickPosition.y * texture.width + currentData.ClickPosition.x].a == 0)
                    return;
                
                if (Settings.UsePattern)
                {
                    filledPixels = new Color32[pixels.Length];
                    for (var i = 0; i < filledPixels.Length; i++)
                    {
                        filledPixels[i] = new Color32(0, 0, 0, 0);
                    }
                }

                drawProcessed = true;
                isRunning = true;
                visitedPixels = new bool[pixels.Length];
                var width = texture.width;
                var height = texture.height;
                var color = Data.Brush.Color;

                if (useThreads)
                {
                    thread = new Thread(() =>
                    {
                        FillTexture(color, width, height, currentData.ClickPosition.x, currentData.ClickPosition.y);
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                {
                    FillTexture(color, width, height, currentData.ClickPosition.x, currentData.ClickPosition.y);
                }
                Data.StartCoroutine(WaitForFillEnd());
            }
            else if (isRunning && (!useThreads || (useThreads && !thread.IsAlive)))
            {
                texture.SetPixels32(pixels);
                texture.Apply();

                if (Settings.UsePattern)
                {
                    var inputTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                    inputTexture.SetPixels32(filledPixels);
                    inputTexture.Apply();
                    Graphics.Blit(inputTexture, GetTexture(RenderTarget.Input));
                    Object.Destroy(inputTexture);

                    Data.Material.SetTexture(Constants.PaintShader.InputTexture, GetTexture(RenderTarget.Input));
                    if (!Data.PaintMode.UsePaintInput)
                    {
                        Data.CommandBuilder.Clear().LoadOrtho().SetRenderTarget(GetTarget(RenderTarget.CombinedTemp)).DrawMesh(Data.QuadMesh, Data.Material, InputToPaintPass).Execute();
                        Graphics.Blit(GetTexture(RenderTarget.CombinedTemp), currentData.Layer);
                    }
                }
                else
                {
                    Graphics.Blit(texture, currentData.Layer);
                }
                
                visitedPixels = null;
                isRunning = false;
                base.OnDrawProcess(combined);
                if (Settings.UsePattern && Data.PaintMode.UsePaintInput)
                {
                    OnBakeInputToLayer(currentData.Layer);
                    Data.CommandBuilder.Clear().LoadOrtho().SetRenderTarget(GetTarget(RenderTarget.Input)).ClearRenderTarget().Execute();
                }
                Data.SaveState();
                currentData = null;
                drawProcessed = true;
            }

            if (!drawProcessed)
            {
                base.OnDrawProcess(combined);
            }
        }
        
        private void ToolSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.UsePattern))
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
        }

        private IEnumerator WaitForFillEnd()
        {
            while (useThreads && thread.IsAlive)
            {
                yield return null;
            }

            if (!Data.IsPainting && !Data.Brush.Preview)
            {
                Data.Render();
            }
        }

        private void FillTexture(Color32 fillColor, int width, int height, int x, int y)
        {
            var targetColor = pixels[x + y * width];
            var eps = Mathf.RoundToInt(Mathf.Clamp(Settings.Tolerance, 0f, 1f) * 127.5f);
            var position = new Vector2Int(x, y);
            var positions = new Stack<Vector2Int>();
            positions.Push(position);
            while (positions.Count > 0)
            {
                position = positions.Pop();

                if (visitedPixels[position.x + position.y * width] || !pixels[position.x + position.y * width].AreColorsSimilar(targetColor, eps))
                    continue;

                var leftX = position.x;
                var rightX = position.x;
                y = position.y;

                while (leftX > 0 && pixels[leftX - 1 + y * width].AreColorsSimilar(targetColor, eps))
                {
                    leftX--;
                }

                while (rightX < width - 1 && pixels[rightX + 1 + y * width].AreColorsSimilar(targetColor, eps))
                {
                    rightX++;
                }

                for (var i = leftX; i <= rightX; i++)
                {
                    if (!visitedPixels[i + y * width])
                    {
                        pixels[i + y * width] = fillColor;
                        if (Settings.UsePattern)
                        {
                            filledPixels[i + y * width] = fillColor;
                        }
                        visitedPixels[i + y * width] = true;

                        if (y > 0 && !visitedPixels[i + (y - 1) * width] && pixels[i + (y - 1) * width].AreColorsSimilar(targetColor, eps))
                        {
                            position.x = i;
                            position.y = y - 1;
                            positions.Push(position);
                        }

                        if (y < height - 1 && !visitedPixels[i + (y + 1) * width] && pixels[i + (y + 1) * width].AreColorsSimilar(targetColor, eps))
                        {
                            position.x = i;
                            position.y = y + 1;
                            positions.Push(position);
                        }
                    }
                }
            }
        }
    }
}