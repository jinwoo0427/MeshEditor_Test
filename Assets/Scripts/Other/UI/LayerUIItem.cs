using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Core;
using XDPaint.Core.Layers;

namespace XDPaint.Demo.UI
{
    public class LayerUIItem : MonoBehaviour
    {
        [SerializeField] private GameObject selection;
        [SerializeField] private Toggle enabledToggle;
        [SerializeField] private InputField nameField;
        [SerializeField] private RawImage texturePreview;
        [SerializeField] private RectTransform textureRectTransform;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Slider opacity;
        [SerializeField] private UIPointerHelper opacityHelper;
        [SerializeField] private LayerDragItem layerDragItem;

        private ILayer layer;
        private Action<ILayer> selectAction;
        private float startOpacityValue;
        private float defaultPreviewWidth;

        public LayerDragItem LayerDragItem => layerDragItem;
        public UIPointerHelper OpacityHelper => opacityHelper;
        public ILayer Layer => layer;

        private void Awake()
        {
            //var blendingData = Enum.GetNames(typeof(BlendingMode));
            //dropdown.options.Clear();
            //// 블랜딩 모드들 드롭다운에 전부 셋팅
            //foreach (var blendingMode in blendingData)
            //{
            //    dropdown.options.Add(new Dropdown.OptionData(blendingMode));
            //}
            defaultPreviewWidth = textureRectTransform.sizeDelta.x;
        }
        
        private void OnEnable()
        {
            enabledToggle.onValueChanged.AddListener(OnToggle);
            nameField.onValueChanged.AddListener(OnTextField);
            dropdown.onValueChanged.AddListener(OnDropdown);
            opacity.onValueChanged.AddListener(OnSlider);
            opacityHelper.OnDown += OpacityHelperOnDown;
            opacityHelper.OnUp += OpacityHelperOnUp;
        }

        private void OnDisable()
        {
            enabledToggle.onValueChanged.RemoveListener(OnToggle);
            nameField.onValueChanged.RemoveListener(OnTextField);
            dropdown.onValueChanged.RemoveListener(OnDropdown);
            opacity.onValueChanged.RemoveListener(OnSlider);
            opacityHelper.OnDown -= OpacityHelperOnDown;
            opacityHelper.OnUp -= OpacityHelperOnUp;
        }

        private void OnDestroy()
        {
            if (layer != null)
            {
                layer.OnLayerChanged -= OnLayerChanged;
            }
        }

        public void SetLayer(ILayer layerData)
        {
            if (layer != null)
            {
                layer.OnLayerChanged -= OnLayerChanged;
            }
            layer = layerData;
            layer.OnLayerChanged -= OnLayerChanged;
            layer.OnLayerChanged += OnLayerChanged;
            OnLayerChanged(layer);
            texturePreview.texture = layer.RenderTexture;
            
            var width = layer.RenderTexture.width;
            var height = layer.RenderTexture.height;
            float aspect;
            if (width >= height)
            {
                aspect = width / (float)height;
                textureRectTransform.sizeDelta = new Vector2(defaultPreviewWidth, defaultPreviewWidth / aspect);
            }
            else
            {
                aspect = height / (float)width;
                textureRectTransform.sizeDelta = new Vector2(defaultPreviewWidth / aspect, defaultPreviewWidth);
            }
            nameField.text = layer.Name;
        }

        private void OnLayerChanged(ILayer changedLayer)
        {
            enabledToggle.interactable = changedLayer.CanBeDisabled;
            enabledToggle.isOn = changedLayer.Enabled;
            nameField.text = changedLayer.Name;
            texturePreview.texture = changedLayer.RenderTexture;
            //dropdown.value = (int)changedLayer.BlendingMode;
            opacity.value = changedLayer.Opacity;
        }

        public void SetEnableInteractableState(bool isInteractable)
        {
            enabledToggle.interactable = isInteractable;
        }

        public void SetSelection(Action<ILayer> onSelection)
        {
            selectAction = onSelection;
        }

        public void SetActive()
        {
            selection.SetActive(true);
            selectAction?.Invoke(layer);
        }

        public void SetInactive()
        {
            selection.SetActive(false);
        }

        private void OnToggle(bool isChecked)
        {
            layer.Enabled = isChecked;
        }

        private void OnTextField(string text)
        {
            layer.Name = text;
        }
        
        private void OnDropdown(int index)
        {
            //layer.BlendingMode = ((BlendingMode)index);
        }

        private void OnSlider(float value)
        {
            layer.Opacity = value;
        }
        
        private void OpacityHelperOnDown(PointerEventData pointer)
        {
            startOpacityValue = opacity.value;
        }
        
        private void OpacityHelperOnUp(PointerEventData pointer)
        {
            if (opacity.value != startOpacityValue)
            {
                layer.Opacity = opacity.value;
            }
        }
    }
}