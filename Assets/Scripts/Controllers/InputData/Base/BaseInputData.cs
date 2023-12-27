using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;
using XDPaint.Tools.Raycast.Data;
using IDisposable = XDPaint.Core.IDisposable;

namespace XDPaint.Controllers.InputData.Base
{
    public abstract class BaseInputData : IDisposable
    {
        public event Action<int, Vector3, RaycastData> OnHoverSuccessHandler;
        public event Action<int, Vector3, RaycastData> OnHoverFailedHandler;
        public event Action<int, Vector3, float, RaycastData> OnDownHandler;
        public event Action<int, Vector3, float, RaycastData> OnDownFailedHandler;
        public event Action<int, Vector3, float, RaycastData> OnPressHandler;
        public event Action<int, Vector3, float, RaycastData> OnPressFailedHandler;
        public event Action<int, Vector3> OnUpHandler;

        protected Camera Camera;
        protected PaintManager PaintManager;
        protected MeshModifyManager MeshManager;
        private List<CanvasGraphicRaycaster> raycasters;
        private Dictionary<int, Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>> raycastResults;
        private bool canHover = true;
        private bool isOnDownSuccess;
        public virtual void Init(MeshModifyManager meshManagerInstance, Camera camera)
        {
            Camera = camera;
            raycasters = new List<CanvasGraphicRaycaster>();
            MeshManager = meshManagerInstance;
            raycastResults = new Dictionary<int, Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>>();
            for (var i = 0; i < InputController.Instance.MaxTouchesCount; i++)
            {
                raycastResults.Add(i, new Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>());
            }
            if (Settings.Instance.CheckCanvasRaycasts)
            {
                foreach (var canvas in InputController.Instance.Canvases)
                {
                    if (canvas == null)
                        continue;

                    if (canvas != null)
                    {
                        if (!canvas.TryGetComponent<CanvasGraphicRaycaster>(out var graphicRaycaster))
                        {
                            graphicRaycaster = canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                        }

                        if (!raycasters.Contains(graphicRaycaster))
                        {
                            raycasters.Add(graphicRaycaster);
                        }
                    }
                }
            }

        }
        public virtual void Init(PaintManager paintManagerInstance, Camera camera)
        {
            Camera = camera;
            PaintManager = paintManagerInstance;
            raycasters = new List<CanvasGraphicRaycaster>();
            raycastResults = new Dictionary<int, Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>>();
            for (var i = 0; i < InputController.Instance.MaxTouchesCount; i++)
            {
                raycastResults.Add(i, new Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>());
            }
            
            if (Settings.Instance.CheckCanvasRaycasts)
            {
                if (PaintManager.ObjectForPainting.TryGetComponent<RawImage>(out var rawImage) && rawImage.canvas != null)
                {
                    if (!rawImage.canvas.TryGetComponent<CanvasGraphicRaycaster>(out var graphicRaycaster))
                    {
                        graphicRaycaster = rawImage.canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                    }
                    if (!raycasters.Contains(graphicRaycaster))
                    {
                        raycasters.Add(graphicRaycaster);
                    }
                }

                foreach (var canvas in InputController.Instance.Canvases)
                {
                    if (canvas == null)
                        continue;

                    if (canvas != null)
                    {
                        if (!canvas.TryGetComponent<CanvasGraphicRaycaster>(out var graphicRaycaster))
                        {
                            graphicRaycaster = canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                        }
                        
                        if (!raycasters.Contains(graphicRaycaster))
                        {
                            raycasters.Add(graphicRaycaster);
                        }
                    }
                }
            }
        }
        
        public virtual void DoDispose()
        {
            raycasters.Clear();
            raycastResults.Clear();
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnHover(int fingerId, Vector3 position)
        {
            if (PaintManager != null)
            {
                if (!CanProcess())
                {
                    OnHoverFailed(fingerId);
                    return;
                }
            }
            
            
            if (Settings.Instance.CheckCanvasRaycasts && raycasters.Count > 0)
            {
                raycastResults[fingerId].Clear();
                foreach (var raycaster in raycasters)
                {
                    var result = raycaster.GetRaycasts(position);
                    if (result != null)
                    {
                        raycastResults[fingerId].Add(raycaster, result);
                    }
                }

                if (canHover && (raycastResults[fingerId].Count == 0 || CheckRaycasts(fingerId)))
                {
                    OnHoverSuccess(fingerId, position, null);
                }
                else
                {
                    OnHoverFailed(fingerId);
                }
            }
            else
            {
                OnHoverSuccess(fingerId, position, null);
            }
        }
        
        protected virtual void OnHoverSuccess(int fingerId, Vector3 position, RaycastData raycast)
        {
            OnHoverSuccessHandlerInvoke(fingerId, position, raycast);
        }
        
        protected virtual void OnHoverSuccessHandlerInvoke(int fingerId, Vector3 position, RaycastData raycast)
        {
            OnHoverSuccessHandler?.Invoke(fingerId, position, raycast);
        }
        
        protected virtual void OnHoverFailed(int fingerId)
        {
            OnHoverFailedHandler?.Invoke(fingerId, Vector4.zero, null);
        }

        public virtual void OnDown(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if (PaintManager != null)
            {
                if (!CanProcess())
                {
                    OnDownFailed(fingerId, position, pressure);
                    return;
                }
            }

            if (Settings.Instance.CheckCanvasRaycasts && raycasters.Count > 0)
            {
                raycastResults[fingerId].Clear();
                foreach (var raycaster in raycasters)
                {
                    var result = raycaster.GetRaycasts(position);
                    if (result != null)
                    {
                        raycastResults[fingerId].Add(raycaster, result);
                    }
                }
                
                if (raycastResults[fingerId].Count == 0 || CheckRaycasts(fingerId))
                {
                    isOnDownSuccess = true;
                    OnDownSuccess(fingerId, position, pressure);
                }
                else
                {
                    canHover = false;
                    isOnDownSuccess = false;
                    OnDownFailed(fingerId, position, pressure);
                }
            }
            else
            {
                isOnDownSuccess = true;
                OnDownSuccess(fingerId, position, pressure);
            }
        }
        
        protected virtual void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            OnDownSuccessInvoke(fingerId, position, pressure);
        }

        protected virtual void OnDownSuccessInvoke(int fingerId, Vector3 position, float pressure = 1.0f, RaycastData raycast = null)
        {
            OnDownHandler?.Invoke(fingerId, position, pressure, raycast);
        }
        
        protected virtual void OnDownFailed(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            OnDownFailedHandler?.Invoke(fingerId, position, pressure, null);
        }

        public virtual void OnPress(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if(PaintManager != null)
            {
                if (!CanProcess())
                {
                    OnPressFailed(fingerId, position, pressure);
                    return;
                }
            }
            

            if (Settings.Instance.CheckCanvasRaycasts && InputController.Instance.BlockRaycastsOnPress && raycasters.Count > 0)
            {
                raycastResults[fingerId].Clear();
                foreach (var raycaster in raycasters)
                {
                    var result = raycaster.GetRaycasts(position);
                    if (result != null)
                    {
                        raycastResults[fingerId].Add(raycaster, result);
                    }
                }

                if (raycastResults[fingerId].Count == 0 || CheckRaycasts(fingerId))
                {
                    OnPressSuccess(fingerId, position, pressure);
                }
                else
                {
                    OnPressFailed(fingerId, position, pressure);
                }
            }
            else if (isOnDownSuccess)
            {
                OnPressSuccess(fingerId, position, pressure);
            }
        }

        protected virtual void OnPressSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            OnPressSuccessInvoke(fingerId, position, pressure);
        }

        protected virtual void OnPressSuccessInvoke(int fingerId, Vector3 position, float pressure = 1.0f, RaycastData raycast = null)
        {
            OnPressHandler?.Invoke(fingerId, position, pressure, raycast);
        }
        
        protected virtual void OnPressFailed(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            OnPressFailedHandler?.Invoke(fingerId, position, pressure, null);
        }

        public virtual void OnUp(int fingerId, Vector3 position)
        {
            if (PaintManager != null)
            {
                if (!CanProcess() )
                    return;
            }
            

            if (isOnDownSuccess)
            {
                OnUpSuccessInvoke(fingerId, position);
            }
            canHover = true;
        }

        protected virtual void OnUpSuccessInvoke(int fingerId, Vector3 position)
        {
            OnUpHandler?.Invoke(fingerId, position);
        }

        private  bool CheckRaycasts(int fingerId)
        {
            var result = true;
            if (fingerId < raycastResults.Count)
            {
                var ignoreRaycasts = InputController.Instance.IgnoreForRaycasts;
                foreach (var raycaster in raycastResults[fingerId].Keys)
                {
                    if (raycastResults[fingerId][raycaster].Count > 0)
                    {
                        var raycast = raycastResults[fingerId][raycaster][0];
                        if (PaintManager != null)
                        {
                            if (raycast.gameObject == PaintManager.ObjectForPainting && PaintManager.Initialized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (raycast.gameObject == MeshManager.ObjectForModifying&& MeshManager.Initialized)
                            {
                                continue;
                            }
                        }
                        

                        if (!ignoreRaycasts.Contains(raycast.gameObject))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        
        private bool CanProcess(bool printWarnings = false)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled ||
                !PaintManager.LayersController.ActiveLayer.Enabled || PaintManager.Brush.Color.a == 0f)
            {
                if (printWarnings)
                {
                    if (!PaintManager.LayersController.ActiveLayer.Enabled)
                    {
                        Debug.LogWarning("Active layer is disabled!");
                    }
                    else if (PaintManager.Brush.Color.a == 0f)
                    {
                        Debug.LogWarning("Brush has zero alpha value!");
                    }
                }
                return false;
            }
            return true;
        }
    }
}