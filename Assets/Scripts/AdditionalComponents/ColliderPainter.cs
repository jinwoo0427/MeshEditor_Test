using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core;

namespace XDPaint.AdditionalComponents
{
    public class ColliderPainter : MonoBehaviour
    {
        public event Action<PaintManager, Collision> OnCollide;
        public Color Color = Color.white;
        public float Pressure = 1f;
        public float PaintDistance = 0.05f;
        public int FingerId;
        
        private readonly Dictionary<PaintManager, PaintState> paintStates = new Dictionary<PaintManager, PaintState>();
        private RaycastHit[] raycastHits;

        private void Awake()
        {
            raycastHits = new RaycastHit[16];
        }

        private void OnCollisionEnter(Collision collision)
        {
            DrawPoint(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            DrawPoint(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            for (var i = paintStates.Keys.Count - 1; i >= 0; i--)
            {
                var key = paintStates.Keys.ElementAt(i);
                var paintState = paintStates[key];
                if (paintState.CollisionTransform == collision.transform)
                {
                    paintStates.Remove(key);
                }
            }
        }

        
        private void DrawPoint(Collision collision)
        {
            var allPaintManagers = PaintController.Instance.AllPaintManagers();
            foreach (var paintManager in allPaintManagers)
            {
                if (paintManager.ObjectForPainting == collision.gameObject)
                {
                    var screenPosition = paintManager.Camera.WorldToScreenPoint(collision.contacts[0].point);

                    Vector2? texturePosition = null;
                    if (paintManager.ComponentType == ObjectComponentType.MeshFilter ||
                        paintManager.ComponentType == ObjectComponentType.SkinnedMeshRenderer)
                    {
                        var ray = new Ray(collision.contacts[0].point + collision.contacts[0].normal * 0.01f, -collision.contacts[0].normal);
                        var raycastsCount = Physics.RaycastNonAlloc(ray, raycastHits, PaintDistance);
                        if (raycastsCount == 0)
                            return;
                        
                        for (var i = 0; i < raycastsCount; i++)
                        {
                            var hit = raycastHits[i];
                            var distance = Vector3.Distance(collision.contacts[0].point, hit.point);
                            if (distance > PaintDistance
                                // || Vector3.Dot(ray.direction, hit.normal) > -1f + float.Epsilon)
                                )
                                continue;
                        
                            var sourceTexture = paintManager.Material.SourceTexture;
                            texturePosition = new Vector2(hit.textureCoord.x * sourceTexture.width, hit.textureCoord.y * sourceTexture.height);
                            break;
                        }

                        if (texturePosition == null)
                        {
                            OnCollisionExit(collision);
                        }
                    }
                    else
                    {
                        texturePosition = paintManager.PaintObject.GetPaintPosition(FingerId, screenPosition);
                    }
  
                    if (paintStates.ContainsKey(paintManager))
                    {
                        paintStates[paintManager].CollisionTransform = collision.transform;
                    }
                    else
                    {
                        paintStates.Add(paintManager, new PaintState { CollisionTransform = collision.transform });
                    }
                    
                    if (texturePosition != null)
                    {
                        var previousBrushColor = paintManager.Brush.Color;
                        paintManager.Brush.SetColor(Color, false, false);
                        OnCollide?.Invoke(paintManager, collision);
                        
                        if (paintStates[paintManager].PreviousTexturePosition != null)
                        {
                            paintManager.PaintObject.DrawLine(paintStates[paintManager].PreviousTexturePosition.Value, texturePosition.Value, Pressure, Pressure, FingerId);
                        }
                        else
                        {
                            paintManager.PaintObject.DrawPoint(texturePosition.Value, Pressure, FingerId);
                        }

                        paintManager.Brush.SetColor(previousBrushColor, false, false);
                        
                        paintStates[paintManager].PreviousTexturePosition = texturePosition.Value;
                    }
                    break;
                }
            }
        }
        
        private class PaintState
        {
            public Transform CollisionTransform;
            public Vector2? PreviousTexturePosition;
        }
    }
}