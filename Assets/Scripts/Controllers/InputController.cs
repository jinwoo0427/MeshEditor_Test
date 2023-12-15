// #define XDPAINT_VR_ENABLE


using System;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools;
using XDPaint.Utils;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace XDPaint.Controllers
{
    public class InputController : Singleton<InputController>
    {
        [Header("General")]
        [SerializeField, Min(1)] private int maxTouchesCount = 10;
        
        [Header("Ignore Raycasts Settings")]
        [SerializeField] private List<Canvas> canvases;
        [SerializeField] private bool blockRaycastsOnPress;
        [SerializeField] private GameObject[] ignoreForRaycasts;
        
        [Header("VR Settings")]
        public Transform PenTransform;

        public event Action OnUpdate;
        public event Action<int, Vector3> OnMouseHover;
        public event Action<int, Vector3, float> OnMouseDown;
        public event Action<int, Vector3, float> OnMouseButton;
        public event Action<int, Vector3> OnMouseUp;

        public int MaxTouchesCount => maxTouchesCount;
        public IList<Canvas> Canvases => canvases;
        public bool BlockRaycastsOnPress => blockRaycastsOnPress;

        public GameObject[] IgnoreForRaycasts => ignoreForRaycasts;

        private bool isVRMode;
        private bool[] isBegan;

#if XDP_DEBUG
        public void OnUpdateCustom()
        {
            OnUpdate?.Invoke();
        }

        public void OnMouseDownCustom(int fingerId, Vector2 screenPosition, float pressure = 1f)
        {
            OnMouseDown?.Invoke(fingerId, screenPosition, pressure);
        }

        public void OnMouseButtonCustom(int fingerId, Vector2 screenPosition, float pressure = 1f)
        {
            OnMouseButton?.Invoke(fingerId, screenPosition, pressure);
        }

        public void OnMouseUpCustom(int fingerId, Vector2 screenPosition)
        {
            OnMouseUp?.Invoke(fingerId, screenPosition);
        }
#endif
        
        void Start()
        {
            isBegan = new bool[maxTouchesCount];

#if ENABLE_INPUT_SYSTEM
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
#endif
        }


        
        void Update()
        {
            // Mouse
#if ENABLE_INPUT_SYSTEM
            
            if (Mouse.current != null)
            {
                OnUpdate?.Invoke();

                var mousePosition = Mouse.current.position.ReadValue();
                OnMouseHover?.Invoke(0, mousePosition);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    OnMouseDown?.Invoke(0, mousePosition, 1f);
                    return;
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    OnMouseButton?.Invoke(0, mousePosition, 1f);
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    OnMouseUp?.Invoke(0, mousePosition);
                }
            }
            else
            {
               
#elif ENABLE_LEGACY_INPUT_MANAGER
                //Touch / Mouse
                if (Input.touchSupported && Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                    {
                        var fingerId = touch.fingerId;
                        if (fingerId >= maxTouchesCount)
                            continue;
                        
                        OnUpdate?.Invoke();

                        var pressure = Settings.Instance.PressureEnabled ? touch.pressure : 1f;
                        
                        if (touch.phase == TouchPhase.Began && !isBegan[fingerId])
                        {
                            isBegan[fingerId] = true;
                            OnMouseDown?.Invoke(fingerId, touch.position, pressure);
                        }

                        if (touch.fingerId == fingerId)
                        {
                            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                            {
                                OnMouseButton?.Invoke(fingerId, touch.position, pressure);
                            }
                            
                            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            {
                                isBegan[fingerId] = false;
                                OnMouseUp?.Invoke(fingerId, touch.position);
                            }
                        }
                    }
                }
                else
                {
                    OnUpdate?.Invoke();

                    OnMouseHover?.Invoke(0, Input.mousePosition);

                    if (Input.GetMouseButtonDown(0))
                    {
                        OnMouseDown?.Invoke(0, Input.mousePosition, 1f);
                        return;
                    }

                    if (Input.GetMouseButton(0))
                    {
                        OnMouseButton?.Invoke(0, Input.mousePosition, 1f);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        OnMouseUp?.Invoke(0, Input.mousePosition);
                    }
                }
#endif
            }
        }
    }
}