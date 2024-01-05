using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ColorPickerUtil
{
    public class PickBar : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [SerializeField] RectTransform indicator;
        public ColorPickerFloatEvent onValueChanged;

        [HideInInspector] public ColorPicker.Mode mode;
        [HideInInspector] public Vector2 pickAreaValue;

        RectTransform rectTransform;
        Texture2D texture;
        Vector2 m_Offset;

        float m_value;
        public float value
        {
            set { m_value = Mathf.Clamp01(value); Refresh(); }
            get { return m_value; }
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public void Initialize()
        {
            if (rectTransform != null) return;
            rectTransform = GetComponent<RectTransform>();
            int height = (int)Mathf.Clamp(rectTransform.rect.height * 0.25f, 0.0f, 512.0f);
            texture = new Texture2D(1, height);
            texture.wrapMode = TextureWrapMode.Clamp;
            GetComponent<RawImage>().texture = texture;
            transition = Transition.None;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(texture);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Initialize();
            int height = (int)Mathf.Clamp(rectTransform.rect.height * 0.25f, 0.0f, 512.0f);
            if (texture.height != height) texture.height = height;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            base.OnPointerDown(eventData);

            m_Offset = Vector2.zero;
            if (RectTransformUtility.RectangleContainsScreenPoint(indicator, eventData.position, eventData.enterEventCamera))
            {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(indicator, eventData.position, eventData.pressEventCamera, out localMousePos))
                    m_Offset = localMousePos;
            }
            else
            {
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }

        void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, cam, out localCursor)) return;
            localCursor -= rectTransform.rect.position;
            m_value = Mathf.Clamp01((localCursor.y - m_Offset.y) / rectTransform.rect.height);
            onValueChanged.Invoke(m_value);
            Refresh();
        }

        public void Refresh()
        {
            Initialize();
            indicator.anchoredPosition = new Vector2(0.0f, m_value * rectTransform.rect.height);
            switch (mode)
            {
                case ColorPicker.Mode.HSV_H:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorHSV color = new ColorHSV();
                                color.h = (float)y / texture.height * 360.0f;
                                color.s = 1.0f;
                                color.v = 1.0f;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                case ColorPicker.Mode.HSV_S:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorHSV color = new ColorHSV();
                                color.h = pickAreaValue.x * 360.0f;
                                color.s = (float)y / texture.height;
                                color.v = pickAreaValue.y;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                case ColorPicker.Mode.HSV_V:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorHSV color = new ColorHSV();
                                color.h = pickAreaValue.x * 360.0f;
                                color.s = pickAreaValue.y;
                                color.v = (float)y / texture.height;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                case ColorPicker.Mode.RGB_R:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                Color color = new Color();
                                color.r = (float)y / texture.height;
                                color.g = pickAreaValue.y;
                                color.b = pickAreaValue.x;
                                color.a = 1.0f;
                                texture.SetPixel(x, y, color);
                            }
                    }
                    break;
                case ColorPicker.Mode.RGB_G:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                Color color = new Color();
                                color.r = pickAreaValue.y;
                                color.g = (float)y / texture.height;
                                color.b = pickAreaValue.x;
                                color.a = 1.0f;
                                texture.SetPixel(x, y, color);
                            }
                    }
                    break;
                case ColorPicker.Mode.RGB_B:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                Color color = new Color();
                                color.r = pickAreaValue.x;
                                color.g = pickAreaValue.y;
                                color.b = (float)y / texture.height;
                                color.a = 1.0f;
                                texture.SetPixel(x, y, color);
                            }
                    }
                    break;
                case ColorPicker.Mode.Lab_L:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorLab color = new ColorLab();
                                color.L = (float)y / texture.height * 100.0f;
                                color.a = pickAreaValue.x * 255.0f - 128.0f;
                                color.b = pickAreaValue.y * 255.0f - 128.0f;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                case ColorPicker.Mode.Lab_a:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorLab color = new ColorLab();
                                color.L = pickAreaValue.y * 100.0f;
                                color.a = (float)y / texture.height * 255.0f - 128.0f;
                                color.b = pickAreaValue.x * 255.0f - 128.0f;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                case ColorPicker.Mode.Lab_b:
                    {
                        for (int y = 0; y < texture.height; ++y)
                            for (int x = 0; x < texture.width; ++x)
                            {
                                ColorLab color = new ColorLab();
                                color.L = pickAreaValue.y * 100.0f;
                                color.a = pickAreaValue.x * 255.0f - 128.0f;
                                color.b = (float)y / texture.height * 255.0f - 128.0f;
                                color.alpha = 1.0f;
                                texture.SetPixel(x, y, color.ToColor());
                            }
                    }
                    break;
                default:
                    break;
            }
            texture.Apply();
        }

        public void Rebuild(CanvasUpdate executing) { }

        public void LayoutComplete() { }

        public void GraphicUpdateComplete() { }
    }
}